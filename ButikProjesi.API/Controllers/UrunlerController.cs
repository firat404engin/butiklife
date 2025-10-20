using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ButikProjesi.API.Modeller;
using SharedModels = ButikProjesi.Shared.Modeller;

namespace ButikProjesi.API.Controllers
{
    /// <summary>
    /// Ürünler ile ilgili API işlemlerini yöneten controller sınıfı
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UrunlerController : ControllerBase
    {
        private readonly VeriTabaniContext _veriTabaniContext;

        /// <summary>
        /// UrunlerController constructor
        /// </summary>
        /// <param name="veriTabaniContext">Veritabanı bağlam sınıfı</param>
        public UrunlerController(VeriTabaniContext veriTabaniContext)
        {
            _veriTabaniContext = veriTabaniContext;
        }

        /// <summary>
        /// Tüm ürünlerin listesini yorum istatistikleriyle birlikte getirir
        /// </summary>
        /// <returns>UrunDto listesi (ortalama puan ve yorum sayısı dahil)</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SharedModels.UrunDto>>> TumUrunleriGetir(
            [FromQuery] string? q,
            [FromQuery] string? kategori,
            [FromQuery] decimal? minFiyat,
            [FromQuery] decimal? maxFiyat,
            [FromQuery] string? sirala,
            [FromQuery] string? sortBy)
        {
            try
            {
                // Sorguyu oluştur
                var sorgu = _veriTabaniContext.Urunler
                    .Include(u => u.Kategori)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var anahtar = q.Trim().ToLower();
                    sorgu = sorgu.Where(u => u.Ad.ToLower().Contains(anahtar) || (u.Aciklama ?? "").ToLower().Contains(anahtar));
                }
                if (!string.IsNullOrWhiteSpace(kategori))
                {
                    sorgu = sorgu.Where(u => u.Kategori != null && u.Kategori.Ad == kategori);
                }
                if (minFiyat.HasValue)
                {
                    sorgu = sorgu.Where(u => u.Fiyat >= minFiyat.Value);
                }
                if (maxFiyat.HasValue)
                {
                    sorgu = sorgu.Where(u => u.Fiyat <= maxFiyat.Value);
                }

                // Sıralama
                switch ((sirala ?? string.Empty).ToLower())
                {
                    case "fiyatartan":
                        sorgu = sorgu.OrderBy(u => u.Fiyat);
                        break;
                    case "fiyatazalan":
                        sorgu = sorgu.OrderByDescending(u => u.Fiyat);
                        break;
                    case "yeni":
                        sorgu = sorgu.OrderByDescending(u => u.Id);
                        break;
                    default:
                        break;
                }

                var urunler = await sorgu.ToListAsync();

                // Her ürün için yorum istatistiklerini hesapla
                var urunDtoListesi = new List<SharedModels.UrunDto>();
                
                foreach (var urun in urunler)
                {
                    // Sadece ONAYLANMIŞ yorumları al
                    var onayliYorumlar = await _veriTabaniContext.Yorumlar
                        .Where(y => y.UrunId == urun.Id && y.Onaylandi == true)
                        .ToListAsync();

                    var yorumSayisi = onayliYorumlar.Count;
                    var ortalamaPuan = yorumSayisi > 0 
                        ? onayliYorumlar.Average(y => y.Puan) 
                        : 0;

                    // Satiş sayısı: bu ürün kaç siparis kaleminde yer almış
                    var satisSayisi = await _veriTabaniContext.SiparisKalemleri
                        .CountAsync(sk => sk.UrunId == urun.Id);

                    urunDtoListesi.Add(new SharedModels.UrunDto
                    {
                        Id = urun.Id,
                        Ad = urun.Ad,
                        Aciklama = urun.Aciklama,
                        Fiyat = urun.Fiyat,
                        EskiFiyat = urun.EskiFiyat,
                        StokAdedi = urun.StokAdedi,
                        GorselUrl = urun.GorselUrl,
                        KategoriId = urun.KategoriId,
                        Kategori = urun.Kategori == null 
                            ? null 
                            : new SharedModels.Kategori { Id = urun.Kategori.Id, Ad = urun.Kategori.Ad },
                        OrtalamaPuan = ortalamaPuan,
                        YorumSayisi = yorumSayisi,
                        SatisSayisi = satisSayisi
                    });
                }

                // Yeni akıllı sıralama
                switch ((sortBy ?? "encokyorumlanan").ToLower())
                {
                    case "encoksatan":
                        urunDtoListesi = urunDtoListesi
                            .OrderByDescending(u => u.SatisSayisi)
                            .ThenByDescending(u => u.YorumSayisi)
                            .ToList();
                        break;
                    case "eniyipuan":
                        urunDtoListesi = urunDtoListesi
                            .OrderByDescending(u => u.OrtalamaPuan)
                            .ThenByDescending(u => u.YorumSayisi)
                            .ToList();
                        break;
                    case "encokyorumlanan":
                        urunDtoListesi = urunDtoListesi
                            .OrderByDescending(u => u.YorumSayisi)
                            .ThenByDescending(u => u.OrtalamaPuan)
                            .ToList();
                        break;
                    case "fiyatartan":
                        urunDtoListesi = urunDtoListesi
                            .OrderBy(u => u.Fiyat)
                            .ToList();
                        break;
                    case "fiyatazalan":
                        urunDtoListesi = urunDtoListesi
                            .OrderByDescending(u => u.Fiyat)
                            .ToList();
                        break;
                    default:
                        // varsayılan: en çok yorumlanan
                        urunDtoListesi = urunDtoListesi
                            .OrderByDescending(u => u.YorumSayisi)
                            .ThenByDescending(u => u.OrtalamaPuan)
                            .ToList();
                        break;
                }

                return Ok(urunDtoListesi);
            }
            catch (Exception ex)
            {
                // Hata durumunda 500 Internal Server Error döndür
                return StatusCode(500, new { 
                    Hata = "Ürünler getirilirken bir hata oluştu", 
                    Detay = ex.Message 
                });
            }
        }

        /// <summary>
        /// Belirtilen ID'ye sahip ürünü getirir
        /// </summary>
        /// <param name="id">Ürün kimlik numarası</param>
        /// <returns>Ürün bilgisi</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ButikProjesi.API.Modeller.Urun>> UrunGetir(int id)
        {
            try
            {
                // Veritabanından belirtilen ID'ye sahip ürünü kategori bilgisiyle birlikte getir
                var urun = await _veriTabaniContext.Urunler
                    .Include(u => u.Kategori)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (urun == null)
                {
                    // Ürün bulunamadığında 404 Not Found döndür
                    return NotFound(new { 
                        Hata = $"ID'si {id} olan ürün bulunamadı" 
                    });
                }

                return Ok(urun);
            }
            catch (Exception ex)
            {
                // Hata durumunda 500 Internal Server Error döndür
                return StatusCode(500, new { 
                    Hata = "Ürün getirilirken bir hata oluştu", 
                    Detay = ex.Message 
                });
            }
        }

        /// <summary>
        /// Yeni ürün oluşturur (Sadece Admin)
        /// </summary>
        /// <param name="urun">Oluşturulacak ürün bilgileri</param>
        /// <returns>Oluşturulan ürün</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ButikProjesi.API.Modeller.Urun>> UrunOlustur(ButikProjesi.API.Modeller.Urun urun)
        {
            try
            {
                if (urun == null)
                {
                    return BadRequest(new { Hata = "Ürün bilgileri boş olamaz" });
                }

                // Kategori var mı kontrol et
                var kategori = await _veriTabaniContext.Kategoriler
                    .FirstOrDefaultAsync(k => k.Id == urun.KategoriId);
                
                if (kategori == null)
                {
                    return BadRequest(new { Hata = "Geçersiz kategori ID'si" });
                }

                _veriTabaniContext.Urunler.Add(urun);
                await _veriTabaniContext.SaveChangesAsync();

                return CreatedAtAction(nameof(UrunGetir), new { id = urun.Id }, urun);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Hata = "Ürün oluşturulurken bir hata oluştu", 
                    Detay = ex.Message 
                });
            }
        }

        /// <summary>
        /// Mevcut ürünü günceller (Sadece Admin)
        /// </summary>
        /// <param name="id">Güncellenecek ürün ID'si</param>
        /// <param name="urun">Güncellenmiş ürün bilgileri</param>
        /// <returns>Güncellenmiş ürün</returns>
        [HttpPut("{id}")]
        [AllowAnonymous] // Geçici olarak public yapıldı
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ButikProjesi.API.Modeller.Urun>> UrunGuncelle(int id, ButikProjesi.API.Modeller.Urun urun)
        {
            try
            {
                if (id != urun.Id)
                {
                    return BadRequest(new { Hata = "URL ID'si ile ürün ID'si eşleşmiyor" });
                }

                var mevcutUrun = await _veriTabaniContext.Urunler.FindAsync(id);
                if (mevcutUrun == null)
                {
                    return NotFound(new { Hata = $"ID'si {id} olan ürün bulunamadı" });
                }

                // Kategori var mı kontrol et
                var kategori = await _veriTabaniContext.Kategoriler
                    .FirstOrDefaultAsync(k => k.Id == urun.KategoriId);
                
                if (kategori == null)
                {
                    return BadRequest(new { Hata = "Geçersiz kategori ID'si" });
                }

                // Özel Fiyat Güncelleme Mantığı
                if (mevcutUrun.Fiyat != urun.Fiyat)
                {
                    // Eğer fiyat değiştiyse, eski fiyatı EskiFiyat alanına ata
                    mevcutUrun.EskiFiyat = mevcutUrun.Fiyat;
                }
                else
                {
                    // Fiyat değişmediyse, EskiFiyat'ı güncelle (null olabilir)
                    mevcutUrun.EskiFiyat = urun.EskiFiyat;
                }

                // Diğer alanları güncelle
                mevcutUrun.Ad = urun.Ad;
                mevcutUrun.Aciklama = urun.Aciklama;
                mevcutUrun.Fiyat = urun.Fiyat;
                mevcutUrun.StokAdedi = urun.StokAdedi;
                mevcutUrun.KategoriId = urun.KategoriId;
                mevcutUrun.GorselUrl = urun.GorselUrl;

                await _veriTabaniContext.SaveChangesAsync();

                // Eğer fiyat düştüyse (EskiFiyat varsa), favori kullanıcılara bildirim gönder
                if (mevcutUrun.EskiFiyat.HasValue && mevcutUrun.EskiFiyat > mevcutUrun.Fiyat)
                {
                    await BildirimOlusturFavoriKullanicilara(mevcutUrun);
                }

                return Ok(mevcutUrun);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Hata = "Ürün güncellenirken bir hata oluştu", 
                    Detay = ex.Message 
                });
            }
        }

        /// <summary>
        /// Ürünü siler (Sadece Admin)
        /// </summary>
        /// <param name="id">Silinecek ürün ID'si</param>
        /// <returns>Silme işlemi sonucu</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UrunSil(int id)
        {
            try
            {
                var urun = await _veriTabaniContext.Urunler.FindAsync(id);
                if (urun == null)
                {
                    return NotFound(new { Hata = $"ID'si {id} olan ürün bulunamadı" });
                }

                _veriTabaniContext.Urunler.Remove(urun);
                await _veriTabaniContext.SaveChangesAsync();

                return Ok(new { Mesaj = "Ürün başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Hata = "Ürün silinirken bir hata oluştu", 
                    Detay = ex.Message 
                });
            }
        }

        /// <summary>
        /// Tüm kategorileri getirir
        /// </summary>
        /// <returns>Kategori listesi</returns>
        [HttpGet("kategoriler")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SharedModels.Kategori>>> KategorileriGetir()
        {
            try
            {
                var kategoriler = await _veriTabaniContext.Kategoriler
                    .OrderBy(k => k.Ad)
                    .Select(k => new SharedModels.Kategori
                    {
                        Id = k.Id,
                        Ad = k.Ad
                    })
                    .ToListAsync();

                return Ok(kategoriler);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Hata = "Kategoriler getirilirken bir hata oluştu", 
                    Detay = ex.Message 
                });
            }
        }

        /// <summary>
        /// En çok satan ürünleri getirir
        /// </summary>
        /// <param name="limit">Getirilecek ürün sayısı (varsayılan: 4)</param>
        /// <returns>En çok satan ürün listesi</returns>
        [HttpGet("encoksatan")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SharedModels.UrunDto>>> EnCokSatanUrunleriGetir([FromQuery] int limit = 4)
        {
            try
            {
                // Tüm ürünleri ve satış sayılarını getir
                var urunler = await _veriTabaniContext.Urunler
                    .Include(u => u.Kategori)
                    .ToListAsync();

                var urunDtoListesi = new List<SharedModels.UrunDto>();
                
                foreach (var urun in urunler)
                {
                    // Sadece ONAYLANMIŞ yorumları al
                    var onayliYorumlar = await _veriTabaniContext.Yorumlar
                        .Where(y => y.UrunId == urun.Id && y.Onaylandi == true)
                        .ToListAsync();

                    var yorumSayisi = onayliYorumlar.Count;
                    var ortalamaPuan = yorumSayisi > 0 
                        ? onayliYorumlar.Average(y => y.Puan) 
                        : 0;

                    // Satiş sayısı: bu ürün kaç siparis kaleminde yer almış
                    var satisSayisi = await _veriTabaniContext.SiparisKalemleri
                        .CountAsync(sk => sk.UrunId == urun.Id);

                    urunDtoListesi.Add(new SharedModels.UrunDto
                    {
                        Id = urun.Id,
                        Ad = urun.Ad,
                        Aciklama = urun.Aciklama,
                        Fiyat = urun.Fiyat,
                        EskiFiyat = urun.EskiFiyat,
                        StokAdedi = urun.StokAdedi,
                        GorselUrl = urun.GorselUrl,
                        KategoriId = urun.KategoriId,
                        Kategori = urun.Kategori == null 
                            ? null 
                            : new SharedModels.Kategori { Id = urun.Kategori.Id, Ad = urun.Kategori.Ad },
                        OrtalamaPuan = ortalamaPuan,
                        YorumSayisi = yorumSayisi,
                        SatisSayisi = satisSayisi
                    });
                }

                // En çok satan ürünleri sırala ve limit uygula
                var encokSatanUrunler = urunDtoListesi
                    .OrderByDescending(u => u.SatisSayisi)
                    .ThenByDescending(u => u.YorumSayisi)
                    .Take(limit)
                    .ToList();

                return Ok(encokSatanUrunler);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Hata = "En çok satan ürünler getirilirken bir hata oluştu", 
                    Detay = ex.Message 
                });
            }
        }

        /// <summary>
        /// Favori kullanıcılara ürün indirimi bildirimi oluşturur
        /// </summary>
        private async Task BildirimOlusturFavoriKullanicilara(ButikProjesi.API.Modeller.Urun urun)
        {
            try
            {
                // Bu ürünü favoriye ekleyen kullanıcıları bul
                var favoriKullanicilar = await _veriTabaniContext.Favoriler
                    .Where(f => f.UrunId == urun.Id)
                    .Select(f => f.KullaniciId)
                    .Distinct()
                    .ToListAsync();

                if (!favoriKullanicilar.Any())
                    return; // Favori kullanıcı yoksa çık

                var yeniBildirimler = new List<ButikProjesi.API.Modeller.Bildirim>();

                foreach (var kullaniciId in favoriKullanicilar)
                {
                    // Bu kullanıcı için bu ürün hakkında daha önce bildirim oluşturulmuş mu kontrol et
                    var mevcutBildirim = await _veriTabaniContext.Bildirimler
                        .FirstOrDefaultAsync(b => b.KullaniciId == kullaniciId && b.UrunId == urun.Id);

                    if (mevcutBildirim == null)
                    {
                        // Yeni bildirim oluştur
                        var yeniBildirim = new ButikProjesi.API.Modeller.Bildirim
                        {
                            KullaniciId = kullaniciId,
                            UrunId = urun.Id,
                            Mesaj = $"Favori ürününüz '{urun.Ad}' indirime girdi! Eski fiyat: {urun.EskiFiyat:C}, Yeni fiyat: {urun.Fiyat:C}",
                            OkunduMu = false,
                            OlusturmaTarihi = DateTime.UtcNow
                        };

                        yeniBildirimler.Add(yeniBildirim);
                    }
                }

                if (yeniBildirimler.Any())
                {
                    _veriTabaniContext.Bildirimler.AddRange(yeniBildirimler);
                    await _veriTabaniContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Bildirim oluşturma hatası log'lanabilir ama ürün güncelleme işlemini etkilememeli
                Console.WriteLine($"Bildirim oluşturma hatası: {ex.Message}");
            }
        }
    }
}
