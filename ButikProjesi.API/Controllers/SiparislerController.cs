using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ButikProjesi.API.Modeller;

namespace ButikProjesi.API.Controllers
{
    /// <summary>
    /// Sipariş işlemleri için API controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SiparislerController : ControllerBase
    {
        private readonly VeriTabaniContext _context;

        public SiparislerController(VeriTabaniContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm siparişleri getirir (Sadece Admin)
        /// </summary>
        /// <returns>Sipariş listesi (kullanıcı email'leri ile birlikte)</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> TumSiparisleriGetir()
        {
            try
            {
                var siparisler = await _context.Siparisler
                    .Include(s => s.SiparisKalemleri)
                    .ThenInclude(sk => sk.Urun)
                    .OrderByDescending(s => s.SiparisTarihi)
                    .ToListAsync();

                // Her sipariş için kullanıcı email'ini al
                var siparislerDetayli = new List<object>();
                
                foreach (var siparis in siparisler)
                {
                    var kullanici = await _context.Users.FindAsync(siparis.KullaniciId);
                    
                    siparislerDetayli.Add(new
                    {
                        id = siparis.Id,
                        kullaniciId = siparis.KullaniciId,
                        kullaniciEmail = kullanici?.Email ?? "Bilinmiyor",
                        siparisTarihi = siparis.SiparisTarihi,
                        toplamTutar = siparis.ToplamTutar,
                        durum = siparis.Durum,
                        adres = siparis.Adres,
                        siparisKalemleri = siparis.SiparisKalemleri.Select(sk => new
                        {
                            id = sk.Id,
                            urunId = sk.UrunId,
                            urunAd = sk.Urun?.Ad,
                            adet = sk.Adet,
                            fiyat = sk.Fiyat
                        }).ToList()
                    });
                }

                return Ok(siparislerDetayli);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mesaj = "Siparişler getirilirken hata oluştu.", hata = ex.Message });
            }
        }

        /// <summary>
        /// Belirtilen ID'ye sahip siparişi getirir (Admin için detaylı)
        /// </summary>
        /// <param name="id">Sipariş ID'si</param>
        /// <returns>Sipariş detayı</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SiparisGetir(int id)
        {
            try
            {
                var siparis = await _context.Siparisler
                    .Include(s => s.SiparisKalemleri)
                    .ThenInclude(sk => sk.Urun)
                    .ThenInclude(u => u.Kategori)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (siparis == null)
                {
                    return NotFound(new { mesaj = $"ID'si {id} olan sipariş bulunamadı." });
                }

                // Kullanıcı bilgilerini al
                var kullanici = await _context.Users.FindAsync(siparis.KullaniciId);

                // Detaylı sipariş bilgisi döndür
                var siparisDetayli = new
                {
                    id = siparis.Id,
                    kullaniciId = siparis.KullaniciId,
                    kullaniciEmail = kullanici?.Email ?? "Bilinmiyor",
                    kullaniciUserName = kullanici?.UserName ?? "Bilinmiyor",
                    adSoyad = siparis.AdSoyad,
                    telefon = siparis.Telefon,
                    adres = siparis.Adres,
                    siparisTarihi = siparis.SiparisTarihi,
                    toplamTutar = siparis.ToplamTutar,
                    durum = siparis.Durum,
                    teslimTarihi = siparis.TeslimTarihi,
                    siparisKalemleri = siparis.SiparisKalemleri.Select(sk => new
                    {
                        id = sk.Id,
                        urunId = sk.UrunId,
                        urunAd = sk.Urun?.Ad,
                        urunGorselUrl = sk.Urun?.GorselUrl,
                        urunAciklama = sk.Urun?.Aciklama,
                        kategoriAd = sk.Urun?.Kategori?.Ad,
                        adet = sk.Adet,
                        fiyat = sk.Fiyat,
                        toplamFiyat = sk.Adet * sk.Fiyat
                    }).ToList()
                };

                return Ok(siparisDetayli);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mesaj = "Sipariş getirilirken hata oluştu.", hata = ex.Message });
            }
        }

        /// <summary>
        /// Giriş yapmış kullanıcının sipariş geçmişini getirir
        /// </summary>
        /// <returns>Kullanıcının sipariş listesi</returns>
        [HttpGet("gecmisim")]
        [Authorize]
        public async Task<ActionResult> SiparisGecmisimiGetir()
        {
            try
            {
                // O anda giriş yapmış kullanıcının ID'sini al
                var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(kullaniciId))
                {
                    return Unauthorized(new { mesaj = "Kullanıcı oturumu bulunamadı." });
                }

                // Sadece bu kullanıcıya ait siparişleri getir
                var siparisler = await _context.Siparisler
                    .Where(s => s.KullaniciId == kullaniciId)
                    .Include(s => s.SiparisKalemleri)
                    .ThenInclude(sk => sk.Urun)
                    .OrderByDescending(s => s.SiparisTarihi)
                    .ToListAsync();

                // Sipariş listesini oluştur
                var siparisListesi = siparisler.Select(siparis => new
                {
                    id = siparis.Id,
                    siparisTarihi = siparis.SiparisTarihi,
                    toplamTutar = siparis.ToplamTutar,
                    durum = siparis.Durum,
                    adres = siparis.Adres,
                    teslimTarihi = siparis.TeslimTarihi,
                    urunSayisi = siparis.SiparisKalemleri.Count,
                    siparisKalemleri = siparis.SiparisKalemleri.Select(sk => new
                    {
                        urunId = sk.UrunId,
                        urunAd = sk.Urun?.Ad,
                        urunGorselUrl = sk.Urun?.GorselUrl,
                        urunAciklama = sk.Urun?.Aciklama,
                        adet = sk.Adet,
                        fiyat = sk.Fiyat
                    }).ToList()
                }).ToList();

                return Ok(siparisListesi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    mesaj = "Sipariş geçmişi getirilirken hata oluştu.", 
                    hata = ex.Message 
                });
            }
        }

        /// <summary>
        /// Siparişin durumunu günceller (Sadece Admin)
        /// </summary>
        /// <param name="id">Sipariş ID'si</param>
        /// <param name="durumDto">Yeni durum bilgisi</param>
        /// <returns>Güncelleme sonucu</returns>
        [HttpPut("{id}/durum")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SiparisDurumGuncelle(int id, [FromBody] SiparisDurumDto durumDto)
        {
            try
            {
                var siparis = await _context.Siparisler.FindAsync(id);

                if (siparis == null)
                {
                    return NotFound(new { mesaj = $"ID'si {id} olan sipariş bulunamadı." });
                }

                // Eski durumu logla
                var eskiDurum = siparis.Durum;
                
                // Yeni durumu güncelle
                siparis.Durum = durumDto.YeniDurum;
                
                // Eğer durum "Teslim Edildi" ise teslim tarihini kaydet
                if (durumDto.YeniDurum == "Teslim Edildi" && siparis.TeslimTarihi == null)
                {
                    siparis.TeslimTarihi = DateTime.UtcNow;
                }
                
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    basarili = true,
                    mesaj = $"Sipariş durumu '{eskiDurum}' → '{durumDto.YeniDurum}' olarak güncellendi.",
                    siparisId = id,
                    eskiDurum = eskiDurum,
                    yeniDurum = durumDto.YeniDurum,
                    teslimTarihi = siparis.TeslimTarihi
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    basarili = false,
                    mesaj = "Sipariş durumu güncellenirken hata oluştu.", 
                    hata = ex.Message 
                });
            }
        }

        /// <summary>
        /// Yeni sipariş oluşturur (Sadece giriş yapmış kullanıcılar)
        /// </summary>
        /// <param name="siparisDto">Sipariş bilgileri</param>
        /// <returns>Oluşturulan sipariş</returns>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Siparis>> SiparisOlustur([FromBody] SiparisOlusturDto siparisDto)
        {
            try
            {
                // Giriş yapmış kullanıcının ID'sini al
                var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(kullaniciId))
                {
                    return Unauthorized(new { mesaj = "Sipariş oluşturmak için giriş yapmalısınız." });
                }

                Console.WriteLine($"Sipariş oluşturuluyor - Kullanıcı ID: {kullaniciId}");

                // Validasyon kontrolleri
                if (string.IsNullOrWhiteSpace(siparisDto.AdSoyad) ||
                    string.IsNullOrWhiteSpace(siparisDto.Telefon) ||
                    string.IsNullOrWhiteSpace(siparisDto.Eposta) ||
                    string.IsNullOrWhiteSpace(siparisDto.Adres) ||
                    string.IsNullOrWhiteSpace(siparisDto.Sehir) ||
                    string.IsNullOrWhiteSpace(siparisDto.PostaKodu))
                {
                    return BadRequest(new { mesaj = "Lütfen tüm zorunlu alanları doldurun." });
                }

                if (siparisDto.SepetUrunleri == null || !siparisDto.SepetUrunleri.Any())
                {
                    return BadRequest(new { mesaj = "Sepetinizde ürün bulunmamaktadır." });
                }

                // Toplam tutarı hesapla
                decimal toplamTutar = 0;
                foreach (var sepetUrunu in siparisDto.SepetUrunleri)
                {
                    toplamTutar += sepetUrunu.Fiyat * sepetUrunu.Adet;
                }

                // Yeni sipariş oluştur (Gerçek kullanıcı ID'si ile)
                var yeniSiparis = new Siparis
                {
                    KullaniciId = kullaniciId, // Giriş yapmış kullanıcının gerçek ID'si
                    SiparisTarihi = DateTime.Now,
                    ToplamTutar = toplamTutar,
                    Durum = "Hazırlanıyor",
                    AdSoyad = siparisDto.AdSoyad,
                    Telefon = siparisDto.Telefon,
                    Eposta = siparisDto.Eposta,
                    Adres = siparisDto.Adres,
                    Sehir = siparisDto.Sehir,
                    PostaKodu = siparisDto.PostaKodu,
                    SiparisNotu = siparisDto.SiparisNotu,
                    SiparisKalemleri = new List<SiparisKalemi>()
                };

                // Sipariş kalemlerini oluştur
                foreach (var sepetUrunu in siparisDto.SepetUrunleri)
                {
                    // Ürünün veritabanında mevcut olduğunu kontrol et
                    var urun = await _context.Urunler.FindAsync(sepetUrunu.UrunId);
                    if (urun == null)
                    {
                        return BadRequest(new { mesaj = $"ID'si {sepetUrunu.UrunId} olan ürün bulunamadı." });
                    }

                    // Stok kontrolü
                    if (urun.StokAdedi < sepetUrunu.Adet)
                    {
                        return BadRequest(new { mesaj = $"{urun.Ad} ürününde yeterli stok bulunmamaktadır." });
                    }

                    var siparisKalemi = new SiparisKalemi
                    {
                        UrunId = sepetUrunu.UrunId,
                        Adet = sepetUrunu.Adet,
                        Fiyat = sepetUrunu.Fiyat
                    };

                    yeniSiparis.SiparisKalemleri.Add(siparisKalemi);

                    // Stok güncelle
                    urun.StokAdedi -= sepetUrunu.Adet;
                }

                // Siparişi veritabanına kaydet
                _context.Siparisler.Add(yeniSiparis);
                await _context.SaveChangesAsync();

                // Başarı mesajı ile birlikte sipariş ID'sini döndür
                return CreatedAtAction(
                    nameof(SiparisGetir),
                    new { id = yeniSiparis.Id },
                    new
                    {
                        mesaj = "Siparişiniz başarıyla oluşturuldu!",
                        siparisId = yeniSiparis.Id,
                        toplamTutar = yeniSiparis.ToplamTutar,
                        siparisTarihi = yeniSiparis.SiparisTarihi,
                        durum = yeniSiparis.Durum,
                        teslimatBilgileri = new
                        {
                            adSoyad = siparisDto.AdSoyad,
                            telefon = siparisDto.Telefon,
                            eposta = siparisDto.Eposta,
                            adres = siparisDto.Adres,
                            sehir = siparisDto.Sehir,
                            postaKodu = siparisDto.PostaKodu,
                            siparisNotu = siparisDto.SiparisNotu
                        }
                    }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mesaj = "Sipariş oluşturulurken hata oluştu.", hata = ex.Message });
            }
        }

        /// <summary>
        /// Sipariş durumunu günceller
        /// </summary>
        /// <param name="id">Sipariş ID'si</param>
        /// <param name="yeniDurum">Yeni durum</param>
        /// <returns>Güncelleme sonucu</returns>
        [HttpPatch("{id}/durum")]
        public async Task<IActionResult> SiparisDurumGuncelle(int id, [FromBody] string yeniDurum)
        {
            try
            {
                var siparis = await _context.Siparisler.FindAsync(id);
                if (siparis == null)
                {
                    return NotFound(new { mesaj = $"ID'si {id} olan sipariş bulunamadı." });
                }

                siparis.Durum = yeniDurum;
                await _context.SaveChangesAsync();

                return Ok(new { mesaj = "Sipariş durumu başarıyla güncellendi.", yeniDurum = siparis.Durum });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mesaj = "Sipariş durumu güncellenirken hata oluştu.", hata = ex.Message });
            }
        }
    }
}

