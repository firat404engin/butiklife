using System.Text.Json;
using ButikProjesi.Shared.Modeller;

namespace ButikProjesi.Istemci.Servisler
{
    public class MusteriYonetimiServisi
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public MusteriYonetimiServisi(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<MusteriDto>> MusterileriGetirAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/MusteriYonetimi");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var musteriler = JsonSerializer.Deserialize<List<MusteriDto>>(jsonContent, _jsonOptions);
                    return musteriler ?? new List<MusteriDto>();
                }
                else
                {
                    Console.WriteLine($"Müşteriler getirilirken hata: {response.StatusCode}");
                    return new List<MusteriDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşteriler getirilirken hata: {ex.Message}");
                return new List<MusteriDto>();
            }
        }

        public async Task<List<SiparisDto>> MusterininSiparisleriniGetirAsync(string kullaniciId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/MusteriYonetimi/{kullaniciId}/siparisler");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var siparisler = JsonSerializer.Deserialize<List<SiparisDto>>(jsonContent, _jsonOptions);
                    return siparisler ?? new List<SiparisDto>();
                }
                else
                {
                    Console.WriteLine($"Müşteri siparişleri getirilirken hata: {response.StatusCode}");
                    return new List<SiparisDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Müşteri siparişleri getirilirken hata: {ex.Message}");
                return new List<SiparisDto>();
            }
        }
    }

    public class SiparisDto
    {
        public int Id { get; set; }
        public DateTime SiparisTarihi { get; set; }
        public decimal ToplamTutar { get; set; }
        public string Durum { get; set; } = string.Empty;
        public string Adres { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string? Notlar { get; set; }
    }
}
