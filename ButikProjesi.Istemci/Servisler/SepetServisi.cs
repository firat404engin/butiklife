using Blazored.LocalStorage;
using ButikProjesi.Shared.Modeller;
using ButikProjesi.Istemci.Modeller;

namespace ButikProjesi.Istemci.Servisler
{
    /// <summary>
    /// Alışveriş sepeti yönetim servisi
    /// Local Storage kullanarak kalıcı sepet yönetimi
    /// </summary>
    public class SepetServisi
    {
        private readonly ILocalStorageService _localStorage;
        private const string SEPET_KEY = "sepet";

        public SepetServisi(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        /// <summary>
        /// Sepet değiştiğinde tetiklenen event
        /// </summary>
        public event Action? OnChange;

        /// <summary>
        /// Ürünü sepete ekler veya adedini artırır
        /// </summary>
        /// <param name="urun">Eklenecek ürün</param>
        /// <param name="adet">Eklenecek adet (varsayılan: 1)</param>
        public async Task SepeteEkle(Urun urun, int adet = 1)
        {
            try
            {
                var sepet = await SepetiGetir();
                
                var mevcutUrun = sepet.FirstOrDefault(x => x.UrunId == urun.Id);
                
                if (mevcutUrun != null)
                {
                    // Ürün zaten sepette varsa adedini artır
                    mevcutUrun.Adet += adet;
                }
                else
                {
                    // Yeni ürün ekle
                    var yeniSepetUrunu = new SepetUrunu
                    {
                        UrunId = urun.Id,
                        Ad = urun.Ad,
                        Fiyat = urun.Fiyat,
                        GorselUrl = urun.GorselUrl ?? string.Empty,
                        Adet = adet
                    };
                    
                    sepet.Add(yeniSepetUrunu);
                }

                await _localStorage.SetItemAsync(SEPET_KEY, sepet);
                OnChange?.Invoke();
                
                Console.WriteLine($"Sepete eklendi: {urun.Ad} (Adet: {adet})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sepete ekleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Ürünü sepetten çıkarır
        /// </summary>
        /// <param name="urunId">Çıkarılacak ürünün ID'si</param>
        public async Task SepettenCikar(int urunId)
        {
            try
            {
                var sepet = await SepetiGetir();
                var urun = sepet.FirstOrDefault(x => x.UrunId == urunId);
                
                if (urun != null)
                {
                    sepet.Remove(urun);
                    await _localStorage.SetItemAsync(SEPET_KEY, sepet);
                    OnChange?.Invoke();
                    
                    Console.WriteLine($"Sepetten çıkarıldı: {urun.Ad}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sepetten çıkarma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Ürünün adedini günceller
        /// </summary>
        /// <param name="urunId">Güncellenecek ürünün ID'si</param>
        /// <param name="yeniAdet">Yeni adet</param>
        public async Task AdetGuncelle(int urunId, int yeniAdet)
        {
            try
            {
                var sepet = await SepetiGetir();
                var urun = sepet.FirstOrDefault(x => x.UrunId == urunId);
                
                if (urun != null)
                {
                    if (yeniAdet <= 0)
                    {
                        await SepettenCikar(urunId);
                    }
                    else
                    {
                        urun.Adet = yeniAdet;
                        await _localStorage.SetItemAsync(SEPET_KEY, sepet);
                        OnChange?.Invoke();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Adet güncelleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Sepetteki tüm ürünleri getirir
        /// </summary>
        /// <returns>Sepetteki ürünler listesi</returns>
        public async Task<List<SepetUrunu>> SepetiGetir()
        {
            try
            {
                var sepet = await _localStorage.GetItemAsync<List<SepetUrunu>>(SEPET_KEY);
                return sepet ?? new List<SepetUrunu>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sepet getirme hatası: {ex.Message}");
                return new List<SepetUrunu>();
            }
        }

        /// <summary>
        /// Sepetteki toplam ürün adedini getirir
        /// </summary>
        /// <returns>Toplam ürün adedi</returns>
        public async Task<int> ToplamAdet()
        {
            var sepet = await SepetiGetir();
            return sepet.Sum(x => x.Adet);
        }

        /// <summary>
        /// Sepetteki toplam tutarı getirir
        /// </summary>
        /// <returns>Toplam tutar</returns>
        public async Task<decimal> ToplamTutar()
        {
            var sepet = await SepetiGetir();
            return sepet.Sum(x => x.ToplamFiyat);
        }

        /// <summary>
        /// Sepeti tamamen temizler
        /// </summary>
        public async Task SepetiTemizle()
        {
            try
            {
                await _localStorage.RemoveItemAsync(SEPET_KEY);
                OnChange?.Invoke();
                Console.WriteLine("Sepet temizlendi");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sepet temizleme hatası: {ex.Message}");
            }
        }
    }
}
