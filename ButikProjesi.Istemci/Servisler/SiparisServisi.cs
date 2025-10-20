using System.Net.Http.Json;
using System.Text.Json;
using ButikProjesi.Istemci.Modeller;

namespace ButikProjesi.Istemci.Servisler
{
    /// <summary>
    /// Sipariş işlemleri için servis sınıfı
    /// </summary>
    public class SiparisServisi
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public SiparisServisi(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Yeni sipariş oluşturur
        /// </summary>
        /// <param name="siparisDto">Sipariş bilgileri</param>
        /// <returns>Sipariş yanıtı</returns>
        public async Task<SiparisYanitiDto?> SiparisOlusturAsync(SiparisOlusturDto siparisDto)
        {
            try
            {
                Console.WriteLine("Sipariş oluşturuluyor...");
                Console.WriteLine($"API Adresi: {_httpClient.BaseAddress}api/siparisler");
                Console.WriteLine($"Ürün Sayısı: {siparisDto.SepetUrunleri.Count}");
                Console.WriteLine($"Müşteri: {siparisDto.AdSoyad}");

                var response = await _httpClient.PostAsJsonAsync("api/siparisler", siparisDto);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Sipariş başarıyla oluşturuldu: {content}");

                    var yanit = await response.Content.ReadFromJsonAsync<SiparisYanitiDto>(_jsonOptions);
                    return yanit;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Sipariş oluşturma hatası: {response.StatusCode} - {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sipariş servisi hatası: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Tüm siparişleri getirir (Admin için)
        /// </summary>
        /// <returns>Sipariş listesi</returns>
        public async Task<List<System.Text.Json.JsonElement>?> TumSiparisleriGetirAsync()
        {
            try
            {
                Console.WriteLine("Tüm siparişler getiriliyor (Admin)...");
                var response = await _httpClient.GetAsync("api/siparisler");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Sipariş verisi alındı: {jsonContent.Substring(0, Math.Min(100, jsonContent.Length))}...");
                    
                    var siparisler = JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(jsonContent, _jsonOptions);
                    Console.WriteLine($"{siparisler?.Count ?? 0} sipariş getirildi");
                    return siparisler;
                }
                else
                {
                    Console.WriteLine($"Siparişler getirilemedi: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Siparişler getirilirken hata: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Belirtilen ID'ye sahip siparişi getirir (Admin için detaylı)
        /// </summary>
        /// <param name="id">Sipariş ID'si</param>
        /// <returns>Sipariş detayı</returns>
        public async Task<System.Text.Json.JsonElement?> SiparisGetirByIdAsync(int id)
        {
            try
            {
                Console.WriteLine($"Sipariş detayı getiriliyor (Admin): {id}");
                var response = await _httpClient.GetAsync($"api/siparisler/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Sipariş detayı alındı: {jsonContent.Substring(0, Math.Min(200, jsonContent.Length))}...");
                    
                    var siparis = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent, _jsonOptions);
                    Console.WriteLine($"Sipariş detayı başarıyla getirildi: #{id}");
                    return siparis;
                }
                else
                {
                    Console.WriteLine($"Sipariş detayı getirilemedi: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sipariş detayı getirilirken hata: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Siparişin durumunu günceller (Admin için)
        /// </summary>
        /// <param name="id">Sipariş ID'si</param>
        /// <param name="yeniDurum">Yeni durum</param>
        /// <returns>Güncelleme başarılı mı</returns>
        public async Task<bool> SiparisDurumGuncelleAsync(int id, string yeniDurum)
        {
            try
            {
                Console.WriteLine($"Sipariş durumu güncelleniyor: #{id} → {yeniDurum}");
                
                var durumDto = new { yeniDurum = yeniDurum };
                var response = await _httpClient.PutAsJsonAsync($"api/siparisler/{id}/durum", durumDto);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Sipariş durumu güncellendi: {jsonContent}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Sipariş durumu güncellenemedi: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sipariş durumu güncellenirken hata: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Giriş yapmış kullanıcının sipariş geçmişini getirir
        /// </summary>
        /// <returns>Kullanıcının sipariş listesi</returns>
        public async Task<List<System.Text.Json.JsonElement>?> SiparisGecmisimiGetirAsync()
        {
            try
            {
                Console.WriteLine("Sipariş geçmişim getiriliyor...");
                var response = await _httpClient.GetAsync("api/siparisler/gecmisim");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Sipariş geçmişi alındı: {jsonContent.Substring(0, Math.Min(200, jsonContent.Length))}...");
                    
                    var siparisler = JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(jsonContent, _jsonOptions);
                    Console.WriteLine($"{siparisler?.Count ?? 0} sipariş getirildi");
                    return siparisler;
                }
                else
                {
                    Console.WriteLine($"Sipariş geçmişi getirilemedi: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sipariş geçmişi getirilirken hata: {ex.Message}");
                return null;
            }
        }
    }
}

