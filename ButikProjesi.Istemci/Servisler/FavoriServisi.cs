using System.Net.Http.Json;
using System.Text.Json;
using ButikProjesi.Shared.Modeller;

namespace ButikProjesi.Istemci.Servisler
{
    /// <summary>
    /// Favori ürünler için HTTP servisi
    /// </summary>
    public class FavoriServisi
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public FavoriServisi(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Kullanıcının tüm favori ürünlerini getirir
        /// </summary>
        public async Task<List<Urun>?> FavorileriGetirAsync()
        {
            try
            {
                Console.WriteLine("Favoriler getiriliyor...");
                var response = await _httpClient.GetAsync("api/favoriler");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var favoriler = JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(jsonContent, _jsonOptions);
                    
                    // Her favoriden ürün bilgisini çıkar
                    var urunler = new List<Urun>();
                    if (favoriler != null)
                    {
                        foreach (var favori in favoriler)
                        {
                            if (favori.TryGetProperty("urun", out var urunElement))
                            {
                                var urun = JsonSerializer.Deserialize<Urun>(urunElement.GetRawText(), _jsonOptions);
                                if (urun != null)
                                {
                                    urunler.Add(urun);
                                }
                            }
                        }
                    }

                    Console.WriteLine($"{urunler.Count} favori ürün getirildi");
                    return urunler;
                }
                else
                {
                    Console.WriteLine($"Favoriler getirilemedi: {response.StatusCode}");
                    return new List<Urun>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Favoriler getirilirken hata: {ex.Message}");
                return new List<Urun>();
            }
        }

        /// <summary>
        /// Ürünü favorilere ekler
        /// </summary>
        public async Task<(bool basarili, string mesaj)> FavoriEkleAsync(int urunId)
        {
            try
            {
                Console.WriteLine($"Ürün {urunId} favorilere ekleniyor...");
                
                var dto = new { urunId = urunId };
                var response = await _httpClient.PostAsJsonAsync("api/favoriler", dto);

                var jsonContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var sonuc = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent, _jsonOptions);
                    var mesaj = sonuc.GetProperty("mesaj").GetString() ?? "Ürün favorilere eklendi!";
                    Console.WriteLine($"Favori eklendi: {mesaj}");
                    return (true, mesaj);
                }
                else
                {
                    var hata = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent, _jsonOptions);
                    var mesaj = hata.GetProperty("mesaj").GetString() ?? "Favori eklenirken hata oluştu.";
                    Console.WriteLine($"Favori eklenemedi: {mesaj}");
                    return (false, mesaj);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Favori eklenirken hata: {ex.Message}");
                return (false, $"Favori eklenirken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Ürünü favorilerden çıkarır
        /// </summary>
        public async Task<(bool basarili, string mesaj)> FavoriSilAsync(int urunId)
        {
            try
            {
                Console.WriteLine($"Ürün {urunId} favorilerden çıkarılıyor...");
                
                var response = await _httpClient.DeleteAsync($"api/favoriler/{urunId}");

                var jsonContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var sonuc = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent, _jsonOptions);
                    var mesaj = sonuc.GetProperty("mesaj").GetString() ?? "Ürün favorilerden çıkarıldı!";
                    Console.WriteLine($"Favori silindi: {mesaj}");
                    return (true, mesaj);
                }
                else
                {
                    var hata = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent, _jsonOptions);
                    var mesaj = hata.GetProperty("mesaj").GetString() ?? "Favori silinirken hata oluştu.";
                    Console.WriteLine($"Favori silinemedi: {mesaj}");
                    return (false, mesaj);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Favori silinirken hata: {ex.Message}");
                return (false, $"Favori silinirken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Ürünün favorilerde olup olmadığını kontrol eder
        /// </summary>
        public async Task<bool> FavoriKontrolAsync(int urunId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/favoriler/kontrol/{urunId}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var sonuc = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent, _jsonOptions);
                    return sonuc.GetProperty("favoride").GetBoolean();
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Favori kontrolü yapılırken hata: {ex.Message}");
                return false;
            }
        }
    }
}



