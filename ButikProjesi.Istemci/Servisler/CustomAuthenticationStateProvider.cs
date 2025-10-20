using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;
using ButikProjesi.Istemci.Modeller;

namespace ButikProjesi.Istemci.Servisler
{
    /// <summary>
    /// Özel authentication state provider - API'den kullanıcı bilgisi alarak çalışır
    /// </summary>
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ClaimsPrincipal _anonim = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Authentication state'i API'den sorgulayarak getirir
        /// </summary>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                Console.WriteLine("=== GetAuthenticationStateAsync çağrıldı - API'ye istek atılıyor...");
                
                // API'den kullanıcı bilgisini al
                var response = await _httpClient.GetAsync("api/hesap/kullanici-bilgisi");
                
                Console.WriteLine($"=== API Response Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("=== Kullanıcı giriş yapmamış (401/Unauthorized)");
                    return new AuthenticationState(_anonim);
                }

                // API'den gelen yanıtı deserialize et
                var jsonContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"=== API Response JSON: {jsonContent}");
                
                var kullaniciBilgisi = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent);
                
                if (!kullaniciBilgisi.TryGetProperty("basarili", out var basariliProp) || 
                    !basariliProp.GetBoolean())
                {
                    Console.WriteLine("=== API'den başarısız yanıt geldi");
                    return new AuthenticationState(_anonim);
                }

                // Email'i al
                string? email = null;
                if (kullaniciBilgisi.TryGetProperty("email", out var emailProp))
                {
                    email = emailProp.GetString();
                }

                if (string.IsNullOrEmpty(email))
                {
                    Console.WriteLine("=== Email bilgisi bulunamadı");
                    return new AuthenticationState(_anonim);
                }

                Console.WriteLine($"=== Kullanıcı authenticated: {email}");

                // API'den gelen claim'leri al
                var claims = new List<Claim>();
                
                if (kullaniciBilgisi.TryGetProperty("claims", out var claimsProp) && 
                    claimsProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var claimElement in claimsProp.EnumerateArray())
                    {
                        if (claimElement.TryGetProperty("type", out var typeProp) &&
                            claimElement.TryGetProperty("value", out var valueProp))
                        {
                            var type = typeProp.GetString();
                            var value = valueProp.GetString();
                            
                            if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(value))
                            {
                                claims.Add(new Claim(type, value));
                                Console.WriteLine($"=== Claim eklendi: {type} = {value}");
                            }
                        }
                    }
                }

                var identity = new ClaimsIdentity(claims, "apiauth");
                var kullanici = new ClaimsPrincipal(identity);

                Console.WriteLine($"=== ClaimsIdentity oluşturuldu. IsAuthenticated: {identity.IsAuthenticated}");
                Console.WriteLine($"=== Toplam claim sayısı: {claims.Count}");

                return new AuthenticationState(kullanici);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== GetAuthenticationStateAsync HATA: {ex.Message}");
                return new AuthenticationState(_anonim);
            }
        }

        /// <summary>
        /// Kullanıcı girişini bildirir - Tüm uygulama authentication state'ini yeniler
        /// </summary>
        public void NotifyUserAuthentication()
        {
            Console.WriteLine("=== NotifyUserAuthentication çağrıldı - State güncelleniyor...");
            
            // GetAuthenticationStateAsync'i tetikleyerek yeni state'i al
            var authState = GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(authState);
            
            Console.WriteLine("=== Authentication state değişikliği bildirildi!");
        }

        /// <summary>
        /// Kullanıcı çıkışını bildirir - Tüm uygulamayı anonim moda geçirir
        /// </summary>
        public void NotifyUserLogout()
        {
            Console.WriteLine("=== NotifyUserLogout çağrıldı - Anonim moda geçiliyor...");
            
            var authState = Task.FromResult(new AuthenticationState(_anonim));
            NotifyAuthenticationStateChanged(authState);
            
            Console.WriteLine("=== Kullanıcı çıkış yaptı, state anonim olarak güncellendi!");
        }
    }
}
