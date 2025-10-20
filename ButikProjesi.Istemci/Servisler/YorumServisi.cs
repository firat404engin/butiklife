using System.Net.Http.Json;
using System.Text.Json;

namespace ButikProjesi.Istemci.Servisler
{
    /// <summary>
    /// Ürün yorumları için HTTP servisi
    /// </summary>
    public class YorumServisi
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public YorumServisi(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Belirtilen ürüne ait tüm yorumları getirir
        /// </summary>
        /// <param name="urunId">Ürün ID'si</param>
        /// <returns>Yorum listesi</returns>
        public async Task<List<System.Text.Json.JsonElement>?> YorumlariGetirAsync(int urunId)
        {
            try
            {
                Console.WriteLine($"Ürün {urunId} için yorumlar getiriliyor...");
                var response = await _httpClient.GetAsync($"api/yorumlar/urun/{urunId}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Yorum verisi alındı: {jsonContent.Substring(0, Math.Min(200, jsonContent.Length))}...");
                    
                    var yorumlar = JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(jsonContent, _jsonOptions);
                    Console.WriteLine($"{yorumlar?.Count ?? 0} yorum getirildi");
                    return yorumlar;
                }
                else
                {
                    Console.WriteLine($"Yorumlar getirilemedi: {response.StatusCode}");
                    return new List<System.Text.Json.JsonElement>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Yorumlar getirilirken hata: {ex.Message}");
                return new List<System.Text.Json.JsonElement>();
            }
        }

        /// <summary>
        /// Yeni bir yorum ekler
        /// </summary>
        /// <param name="urunId">Ürün ID'si</param>
        /// <param name="metin">Yorum metni</param>
        /// <param name="puan">Puan (1-5)</param>
        /// <param name="isimGosterilsin">Kullanıcının adı görünsün mü?</param>
        /// <returns>Başarılı mı</returns>
        public async Task<(bool basarili, string mesaj)> YorumEkleAsync(int urunId, string metin, int puan, bool isimGosterilsin = true)
        {
            try
            {
                Console.WriteLine($"Ürün {urunId} için yorum ekleniyor...");
                
                var yorumDto = new { urunId = urunId, metin = metin, puan = puan, isimGosterilsin = isimGosterilsin };
                var response = await _httpClient.PostAsJsonAsync("api/yorumlar", yorumDto);

                var jsonContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Yorum ekleme yanıtı: {jsonContent}");

                if (response.IsSuccessStatusCode)
                {
                    var sonuc = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent, _jsonOptions);
                    var mesaj = sonuc.GetProperty("mesaj").GetString() ?? "Yorum başarıyla eklendi!";
                    Console.WriteLine($"Yorum başarıyla eklendi: {mesaj}");
                    return (true, mesaj);
                }
                else
                {
                    var hata = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent, _jsonOptions);
                    var mesaj = hata.GetProperty("mesaj").GetString() ?? "Yorum eklenirken bir hata oluştu.";
                    Console.WriteLine($"Yorum eklenemedi: {mesaj}");
                    return (false, mesaj);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Yorum eklenirken hata: {ex.Message}");
                return (false, $"Yorum eklenirken bir hata oluştu: {ex.Message}");
            }
        }

        /// <summary>
        /// Onay bekleyen tüm yorumları getirir (Sadece Admin)
        /// </summary>
        public async Task<List<System.Text.Json.JsonElement>?> OnayBekleyenleriGetirAsync()
        {
            try
            {
                Console.WriteLine("Onay bekleyen yorumlar getiriliyor...");
                var response = await _httpClient.GetAsync("api/yorumlar/onay-bekleyenler");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var yorumlar = JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(jsonContent, _jsonOptions);
                    Console.WriteLine($"{yorumlar?.Count ?? 0} onay bekleyen yorum getirildi");
                    return yorumlar;
                }
                else
                {
                    Console.WriteLine($"Onay bekleyen yorumlar getirilemedi: {response.StatusCode}");
                    return new List<System.Text.Json.JsonElement>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Onay bekleyen yorumlar getirilirken hata: {ex.Message}");
                return new List<System.Text.Json.JsonElement>();
            }
        }

        /// <summary>
        /// Yorumu onayla (Sadece Admin)
        /// </summary>
        public async Task<(bool basarili, string mesaj)> YorumuOnaylaAsync(int yorumId)
        {
            try
            {
                Console.WriteLine($"Yorum {yorumId} onaylanıyor...");
                var response = await _httpClient.PutAsync($"api/yorumlar/{yorumId}/onayla", null);

                var jsonContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var sonuc = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent, _jsonOptions);
                    var mesaj = sonuc.GetProperty("mesaj").GetString() ?? "Yorum onaylandı!";
                    Console.WriteLine($"Yorum onaylandı: {mesaj}");
                    return (true, mesaj);
                }
                else
                {
                    var hata = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent, _jsonOptions);
                    var mesaj = hata.GetProperty("mesaj").GetString() ?? "Yorum onaylanırken hata oluştu.";
                    Console.WriteLine($"Yorum onaylanamadı: {mesaj}");
                    return (false, mesaj);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Yorum onaylanırken hata: {ex.Message}");
                return (false, $"Yorum onaylanırken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Yorumu sil (Sadece Admin)
        /// </summary>
        public async Task<(bool basarili, string mesaj)> YorumuSilAsync(int yorumId)
        {
            try
            {
                Console.WriteLine($"Yorum {yorumId} siliniyor...");
                var response = await _httpClient.DeleteAsync($"api/yorumlar/{yorumId}");

                var jsonContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var sonuc = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent, _jsonOptions);
                    var mesaj = sonuc.GetProperty("mesaj").GetString() ?? "Yorum silindi!";
                    Console.WriteLine($"Yorum silindi: {mesaj}");
                    return (true, mesaj);
                }
                else
                {
                    var hata = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent, _jsonOptions);
                    var mesaj = hata.GetProperty("mesaj").GetString() ?? "Yorum silinirken hata oluştu.";
                    Console.WriteLine($"Yorum silinemedi: {mesaj}");
                    return (false, mesaj);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Yorum silinirken hata: {ex.Message}");
                return (false, $"Yorum silinirken hata: {ex.Message}");
            }
        }
    }
}

