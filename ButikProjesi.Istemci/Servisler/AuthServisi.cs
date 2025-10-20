using System.Net.Http.Json;
using System.Text.Json;
using ButikProjesi.Istemci.Modeller;

namespace ButikProjesi.Istemci.Servisler
{
    /// <summary>
    /// Authentication işlemleri için servis sınıfı
    /// </summary>
    public class AuthServisi
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuthServisi(
            HttpClient httpClient, 
            CustomAuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Kullanıcı kaydı oluşturur
        /// </summary>
        /// <param name="model">Kayıt bilgileri</param>
        /// <returns>Kayıt sonucu</returns>
        public async Task<AuthYanitDto?> KayitOlAsync(KayitDto model)
        {
            try
            {
                Console.WriteLine($"Kayıt işlemi başlatılıyor: {model.Email}");

                var response = await _httpClient.PostAsJsonAsync("api/hesap/kayitol", model);
                var yanit = await response.Content.ReadFromJsonAsync<AuthYanitDto>(_jsonOptions);

                if (yanit != null && yanit.Basarili)
                {
                    Console.WriteLine($"Kayıt başarılı: {yanit.Email}");
                    
                    // Authentication state'i güncelle
                    _authenticationStateProvider.NotifyUserAuthentication();
                    Console.WriteLine("Authentication state güncellendi (Kayıt)");
                }
                else
                {
                    Console.WriteLine($"Kayıt başarısız: {yanit?.Mesaj}");
                }

                return yanit;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kayıt servisi hatası: {ex.Message}");
                return new AuthYanitDto
                {
                    Basarili = false,
                    Mesaj = "Kayıt işlemi sırasında bir hata oluştu",
                    Hatalar = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Kullanıcı girişi yapar
        /// </summary>
        /// <param name="model">Giriş bilgileri</param>
        /// <returns>Giriş sonucu</returns>
        public async Task<AuthYanitDto?> GirisYapAsync(GirisDto model)
        {
            try
            {
                Console.WriteLine($"Giriş işlemi başlatılıyor: {model.Email}");

                var response = await _httpClient.PostAsJsonAsync("api/hesap/girisyap", model);
                var yanit = await response.Content.ReadFromJsonAsync<AuthYanitDto>(_jsonOptions);

                if (yanit != null && yanit.Basarili)
                {
                    Console.WriteLine($"Giriş başarılı: {yanit.Email}");
                    
                    // Authentication state'i güncelle - Bu çok önemli!
                    _authenticationStateProvider.NotifyUserAuthentication();
                    Console.WriteLine("Authentication state güncellendi (Giriş)");
                }
                else
                {
                    Console.WriteLine($"Giriş başarısız: {yanit?.Mesaj}");
                }

                return yanit;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Giriş servisi hatası: {ex.Message}");
                return new AuthYanitDto
                {
                    Basarili = false,
                    Mesaj = "Giriş işlemi sırasında bir hata oluştu",
                    Hatalar = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Kullanıcı çıkışı yapar
        /// </summary>
        /// <returns>Çıkış sonucu</returns>
        public async Task<AuthYanitDto?> CikisYapAsync()
        {
            try
            {
                Console.WriteLine("Çıkış işlemi başlatılıyor");

                var response = await _httpClient.PostAsync("api/hesap/cikisyap", null);
                var yanit = await response.Content.ReadFromJsonAsync<AuthYanitDto>(_jsonOptions);

                // Authentication state'i sıfırla
                _authenticationStateProvider.NotifyUserLogout();
                Console.WriteLine("Çıkış başarılı, authentication state sıfırlandı");

                return yanit;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Çıkış servisi hatası: {ex.Message}");
                return new AuthYanitDto
                {
                    Basarili = false,
                    Mesaj = "Çıkış işlemi sırasında bir hata oluştu",
                    Hatalar = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Mevcut kullanıcı bilgilerini API'den getirir
        /// </summary>
        /// <returns>Kullanıcı bilgileri</returns>
        public async Task<AuthYanitDto?> KullaniciBilgisiGetirAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/hesap/kullanici-bilgisi");
                
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                
                var yanit = await response.Content.ReadFromJsonAsync<AuthYanitDto>(_jsonOptions);
                return yanit;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kullanıcı bilgisi getir hatası: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Kullanıcının profil bilgilerini getirir
        /// </summary>
        /// <returns>Profil bilgileri (JsonElement)</returns>
        public async Task<System.Text.Json.JsonElement?> GetProfilAsync()
        {
            try
            {
                Console.WriteLine("Profil bilgileri getiriliyor...");
                var response = await _httpClient.GetAsync("api/hesap/profil");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var profil = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent, _jsonOptions);
                    Console.WriteLine("Profil bilgileri başarıyla getirildi");
                    return profil;
                }
                else
                {
                    Console.WriteLine($"Profil bilgileri getirilemedi: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Profil getir hatası: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Kullanıcının profil bilgilerini günceller
        /// </summary>
        /// <param name="adSoyad">Ad Soyad</param>
        /// <param name="adres">Adres</param>
        /// <param name="telefonNumarasi">Telefon Numarası</param>
        /// <returns>Başarılı mı ve mesaj</returns>
        public async Task<(bool basarili, string mesaj)> UpdateProfilAsync(string? adSoyad, string? adres, string? telefonNumarasi)
        {
            try
            {
                Console.WriteLine("Profil güncelleniyor...");
                
                var profilDto = new
                {
                    adSoyad = adSoyad,
                    adres = adres,
                    telefonNumarasi = telefonNumarasi
                };

                var response = await _httpClient.PutAsJsonAsync("api/hesap/profil", profilDto);
                var jsonContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var sonuc = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent, _jsonOptions);
                    var mesaj = sonuc.GetProperty("mesaj").GetString() ?? "Profil başarıyla güncellendi!";
                    Console.WriteLine($"Profil güncellendi: {mesaj}");
                    return (true, mesaj);
                }
                else
                {
                    var hata = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent, _jsonOptions);
                    var mesaj = hata.GetProperty("mesaj").GetString() ?? "Profil güncellenirken bir hata oluştu.";
                    Console.WriteLine($"Profil güncellenemedi: {mesaj}");
                    return (false, mesaj);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Profil güncelleme hatası: {ex.Message}");
                return (false, $"Profil güncellenirken bir hata oluştu: {ex.Message}");
            }
        }
    }
}
