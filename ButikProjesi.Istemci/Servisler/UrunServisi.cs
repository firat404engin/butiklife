using System.Net.Http.Json;
using System.Text.Json;
using ButikProjesi.Shared.Modeller;

namespace ButikProjesi.Istemci.Servisler
{
    /// <summary>
    /// Ürünler ile ilgili API işlemlerini yöneten servis sınıfı
    /// </summary>
    public class UrunServisi
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// UrunServisi constructor
        /// </summary>
        /// <param name="httpClient">HTTP istemci</param>
        public UrunServisi(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// API'den tüm kategorileri getirir
        /// </summary>
        public async Task<List<Kategori>> KategorileriGetirAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/kategoriler");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var kategoriler = JsonSerializer.Deserialize<List<Kategori>>(json, _jsonOptions);
                    return kategoriler ?? new List<Kategori>();
                }
                return new List<Kategori>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kategoriler getirilirken hata oluştu: {ex.Message}");
                return new List<Kategori>();
            }
        }

        public async Task<Kategori?> KategoriEkleAsync(Kategori kategori)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/kategoriler", kategori);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Kategori>(json, _jsonOptions);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kategori eklenirken hata: {ex.Message}");
                return null;
            }
        }

        public async Task<Kategori?> KategoriGuncelleAsync(Kategori kategori)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/kategoriler/{kategori.Id}", kategori);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Kategori>(json, _jsonOptions);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kategori güncellenirken hata: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> KategoriSilAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/kategoriler/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kategori silinirken hata: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// API'den tüm ürünlerin listesini yorum istatistikleriyle birlikte getirir (Müşteri tarafı için)
        /// </summary>
        /// <returns>UrunDto listesi (ortalama puan ve yorum sayısı dahil)</returns>
        public async Task<List<UrunDto>> UrunleriGetirAsync(string? q = null, string? kategori = null, decimal? minFiyat = null, decimal? maxFiyat = null, string? sirala = null)
        {
            try
            {
                var query = new List<string>();
                if (!string.IsNullOrWhiteSpace(q)) query.Add($"q={Uri.EscapeDataString(q)}");
                if (!string.IsNullOrWhiteSpace(kategori)) query.Add($"kategori={Uri.EscapeDataString(kategori)}");
                if (minFiyat.HasValue) query.Add($"minFiyat={minFiyat.Value}");
                if (maxFiyat.HasValue) query.Add($"maxFiyat={maxFiyat.Value}");
                if (!string.IsNullOrWhiteSpace(sirala)) query.Add($"sortBy={Uri.EscapeDataString(sirala)}");
                var qs = query.Count > 0 ? ("?" + string.Join("&", query)) : string.Empty;

                Console.WriteLine($"API URL: {_httpClient.BaseAddress}/api/urunler{qs}");
                var response = await _httpClient.GetAsync($"/api/urunler{qs}");
                Console.WriteLine($"Response Status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    // Başarılı yanıt alındığında JSON'u parse et
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"JSON Content: {jsonContent.Substring(0, Math.Min(200, jsonContent.Length))}...");
                    var urunler = JsonSerializer.Deserialize<List<UrunDto>>(jsonContent, _jsonOptions);
                    Console.WriteLine($"Deserialized ürün sayısı: {urunler?.Count ?? 0}");
                    return urunler ?? new List<UrunDto>();
                }
                else
                {
                    // Hata durumunda boş liste döndür
                    Console.WriteLine($"API Hatası: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<UrunDto>();
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda boş liste döndür ve hatayı logla
                Console.WriteLine($"Ürünler getirilirken hata oluştu: {ex.Message}");
                return new List<UrunDto>();
            }
        }

        /// <summary>
        /// API'den tüm ürünlerin listesini getirir (Admin paneli için - istatistiksiz)
        /// </summary>
        /// <returns>Urun listesi</returns>
        public async Task<List<Urun>> TumUrunleriGetirAsync()
        {
            try
            {
                // UrunDto listesini al
                var urunDtoListesi = await UrunleriGetirAsync();
                
                // UrunDto'dan Urun'e dönüştür
                var urunler = urunDtoListesi.Select(dto => new Urun
                {
                    Id = dto.Id,
                    Ad = dto.Ad,
                    Aciklama = dto.Aciklama,
                    Fiyat = dto.Fiyat,
                    StokAdedi = dto.StokAdedi,
                    GorselUrl = dto.GorselUrl,
                    KategoriId = dto.KategoriId,
                    Kategori = dto.Kategori
                }).ToList();
                
                return urunler;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürünler getirilirken hata oluştu: {ex.Message}");
                return new List<Urun>();
            }
        }

        /// <summary>
        /// API'den belirli bir ürünü getirir
        /// </summary>
        /// <param name="id">Ürün kimlik numarası</param>
        /// <returns>Ürün bilgisi</returns>
        public async Task<Urun?> UrunGetirByIdAsync(int id)
        {
            try
            {
                // API'ye GET isteği gönder
                var response = await _httpClient.GetAsync($"/api/urunler/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    // Başarılı yanıt alındığında JSON'u parse et
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var urun = JsonSerializer.Deserialize<Urun>(jsonContent, _jsonOptions);
                    return urun;
                }
                else
                {
                    // Hata durumunda null döndür
                    Console.WriteLine($"API Hatası: {response.StatusCode} - {response.ReasonPhrase}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda null döndür ve hatayı logla
                Console.WriteLine($"Ürün getirilirken hata oluştu: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Yeni ürün oluşturur
        /// </summary>
        /// <param name="urun">Oluşturulacak ürün</param>
        /// <returns>Oluşturulan ürün</returns>
        public async Task<Urun?> UrunEkleAsync(Urun urun)
        {
            try
            {
                Console.WriteLine($"Yeni ürün ekleniyor: {urun.Ad}");
                
                var response = await _httpClient.PostAsJsonAsync("api/urunler", urun);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var yeniUrun = JsonSerializer.Deserialize<Urun>(jsonContent, _jsonOptions);
                    Console.WriteLine($"Ürün başarıyla eklendi: {yeniUrun?.Ad}");
                    return yeniUrun;
                }
                else
                {
                    Console.WriteLine($"Ürün ekleme hatası: {response.StatusCode} - {response.ReasonPhrase}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün eklenirken hata oluştu: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Mevcut ürünü günceller
        /// </summary>
        /// <param name="urun">Güncellenecek ürün</param>
        /// <returns>Güncellenmiş ürün</returns>
        public async Task<Urun?> UrunGuncelleAsync(Urun urun)
        {
            try
            {
                Console.WriteLine($"Ürün güncelleniyor: {urun.Ad}");
                
                var response = await _httpClient.PutAsJsonAsync($"api/urunler/{urun.Id}", urun);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var guncellenenUrun = JsonSerializer.Deserialize<Urun>(jsonContent, _jsonOptions);
                    Console.WriteLine($"Ürün başarıyla güncellendi: {guncellenenUrun?.Ad}");
                    return guncellenenUrun;
                }
                else
                {
                    Console.WriteLine($"Ürün güncelleme hatası: {response.StatusCode} - {response.ReasonPhrase}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün güncellenirken hata oluştu: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Ürünü siler
        /// </summary>
        /// <param name="id">Silinecek ürün ID'si</param>
        /// <returns>Silme işlemi başarılı mı</returns>
        public async Task<bool> UrunSilAsync(int id)
        {
            try
            {
                Console.WriteLine($"Ürün siliniyor: ID {id}");
                
                var response = await _httpClient.DeleteAsync($"api/urunler/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Ürün başarıyla silindi: ID {id}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Ürün silme hatası: {response.StatusCode} - {response.ReasonPhrase}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün silinirken hata oluştu: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// API'den en çok satan ürünleri getirir
        /// </summary>
        /// <param name="limit">Getirilecek ürün sayısı (varsayılan: 4)</param>
        /// <returns>En çok satan ürün listesi</returns>
        public async Task<List<UrunDto>> EnCokSatanUrunleriGetirAsync(int limit = 4)
        {
            try
            {
                Console.WriteLine($"En çok satan ürünler getiriliyor (limit: {limit})");
                
                var response = await _httpClient.GetAsync($"/api/urunler/encoksatan?limit={limit}");
                Console.WriteLine($"Response Status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"JSON Content: {jsonContent.Substring(0, Math.Min(200, jsonContent.Length))}...");
                    var urunler = JsonSerializer.Deserialize<List<UrunDto>>(jsonContent, _jsonOptions);
                    Console.WriteLine($"Deserialized en çok satan ürün sayısı: {urunler?.Count ?? 0}");
                    return urunler ?? new List<UrunDto>();
                }
                else
                {
                    Console.WriteLine($"API Hatası: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<UrunDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"En çok satan ürünler getirilirken hata oluştu: {ex.Message}");
                return new List<UrunDto>();
            }
        }
    }
}
