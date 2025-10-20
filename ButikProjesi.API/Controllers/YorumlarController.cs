using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ButikProjesi.API.Modeller;

namespace ButikProjesi.API.Controllers
{
    /// <summary>
    /// Ürün yorumları için API controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class YorumlarController : ControllerBase
    {
        private readonly VeriTabaniContext _context;
        private readonly ILogger<YorumlarController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public YorumlarController(VeriTabaniContext context, ILogger<YorumlarController> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        /// <summary>
        /// Belirtilen ürüne ait tüm yorumları getirir
        /// </summary>
        /// <param name="urunId">Ürün ID'si</param>
        /// <returns>Yorum listesi</returns>
        [HttpGet("urun/{urunId}")]
        public async Task<ActionResult> UrunYorumlariGetir(int urunId)
        {
            try
            {
                // Ürünün var olup olmadığını kontrol et
                var urunVarMi = await _context.Urunler.AnyAsync(u => u.Id == urunId);
                if (!urunVarMi)
                {
                    return NotFound(new { mesaj = $"ID'si {urunId} olan ürün bulunamadı." });
                }

                // SADECE ONAYLANMIŞ yorumları getir
                var yorumlar = await _context.Yorumlar
                    .Where(y => y.UrunId == urunId && y.Onaylandi == true)
                    .OrderByDescending(y => y.YorumTarihi)
                    .ToListAsync();

                // Her yorum için kullanıcı bilgisini ekle ve gizlilik ayarına göre adı formatla
                var yorumlarDetayli = new List<object>();
                foreach (var yorum in yorumlar)
                {
                    var kullanici = await _userManager.FindByIdAsync(yorum.KullaniciId);
                    
                    // Gösterilecek adı belirle
                    string gosterilecekAd = "Gizli Kullanıcı";
                    if (kullanici != null)
                    {
                        if (yorum.IsimGosterilsin && !string.IsNullOrEmpty(kullanici.AdSoyad))
                        {
                            // Tam adı göster
                            gosterilecekAd = kullanici.AdSoyad;
                        }
                        else if (!string.IsNullOrEmpty(kullanici.AdSoyad))
                        {
                            // Baş harfleri göster (örn: "Fırat Engin" -> "F. E.")
                            var parcalar = kullanici.AdSoyad.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            if (parcalar.Length >= 2)
                            {
                                gosterilecekAd = $"{parcalar[0][0]}. {parcalar[1][0]}.";
                            }
                            else if (parcalar.Length == 1)
                            {
                                gosterilecekAd = $"{parcalar[0][0]}.";
                            }
                        }
                        else
                        {
                            // AdSoyad yoksa email'den baş harf al
                            gosterilecekAd = !string.IsNullOrEmpty(kullanici.Email) 
                                ? $"{kullanici.Email[0]}." 
                                : "Gizli Kullanıcı";
                        }
                    }
                    
                    yorumlarDetayli.Add(new
                    {
                        id = yorum.Id,
                        urunId = yorum.UrunId,
                        gosterilecekAd = gosterilecekAd,
                        metin = yorum.Metin,
                        puan = yorum.Puan,
                        yorumTarihi = yorum.YorumTarihi
                    });
                }

                _logger.LogInformation("Ürün {UrunId} için {YorumSayisi} yorum getirildi", urunId, yorumlarDetayli.Count);
                return Ok(yorumlarDetayli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün {UrunId} için yorumlar getirilirken hata oluştu", urunId);
                return StatusCode(500, new { mesaj = "Yorumlar getirilirken hata oluştu.", hata = ex.Message });
            }
        }

        /// <summary>
        /// Yeni bir yorum ekler (Sadece giriş yapmış ve ürünü satın almış kullanıcılar)
        /// </summary>
        /// <param name="yorumDto">Yorum bilgileri</param>
        /// <returns>Eklenen yorum</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> YorumEkle([FromBody] YorumEkleDto yorumDto)
        {
            try
            {
                // Giriş yapmış kullanıcının ID'sini al
                var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(kullaniciId))
                {
                    return Unauthorized(new { mesaj = "Kullanıcı oturumu bulunamadı." });
                }

                // Ürünün var olup olmadığını kontrol et
                var urunVarMi = await _context.Urunler.AnyAsync(u => u.Id == yorumDto.UrunId);
                if (!urunVarMi)
                {
                    return NotFound(new { mesaj = $"ID'si {yorumDto.UrunId} olan ürün bulunamadı." });
                }

                // Kullanıcının bu ürünü satın alıp almadığını kontrol et
                var urunuSatinAldiMi = await _context.Siparisler
                    .Where(s => s.KullaniciId == kullaniciId && s.Durum == "Teslim Edildi")
                    .AnyAsync(s => s.SiparisKalemleri.Any(sk => sk.UrunId == yorumDto.UrunId));

                if (!urunuSatinAldiMi)
                {
                    return BadRequest(new { 
                        basarili = false,
                        mesaj = "Sadece satın aldığınız ürünlere yorum yapabilirsiniz." 
                    });
                }

                // Kullanıcının bu ürün için daha önce yorum yapıp yapmadığını kontrol et
                var mevcutYorum = await _context.Yorumlar
                    .AnyAsync(y => y.UrunId == yorumDto.UrunId && y.KullaniciId == kullaniciId);

                if (mevcutYorum)
                {
                    return BadRequest(new { 
                        basarili = false,
                        mesaj = "Bu ürün için zaten yorum yaptınız." 
                    });
                }

                // Yeni yorumu oluştur (Onaylandi = false varsayılan)
                var yeniYorum = new Yorum
                {
                    UrunId = yorumDto.UrunId,
                    KullaniciId = kullaniciId,
                    Metin = yorumDto.Metin,
                    Puan = yorumDto.Puan,
                    IsimGosterilsin = yorumDto.IsimGosterilsin,
                    YorumTarihi = DateTime.Now,
                    Onaylandi = false // Yeni yorumlar onay bekliyor
                };

                _context.Yorumlar.Add(yeniYorum);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Kullanıcı {KullaniciId} ürün {UrunId} için yorum ekledi", kullaniciId, yorumDto.UrunId);

                return Ok(new
                {
                    basarili = true,
                    mesaj = "Yorumunuz başarıyla eklendi!",
                    yorum = new
                    {
                        id = yeniYorum.Id,
                        urunId = yeniYorum.UrunId,
                        metin = yeniYorum.Metin,
                        puan = yeniYorum.Puan,
                        yorumTarihi = yeniYorum.YorumTarihi
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yorum eklenirken hata oluştu");
                return StatusCode(500, new { 
                    basarili = false,
                    mesaj = "Yorum eklenirken hata oluştu.", 
                    hata = ex.Message 
                });
            }
        }

        /// <summary>
        /// Tüm yorumları getirir (Sadece Admin)
        /// </summary>
        [HttpGet("tum-yorumlar")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> TumYorumlariGetir()
        {
            try
            {
                var yorumlar = await _context.Yorumlar
                    .Include(y => y.Urun)
                    .OrderByDescending(y => y.YorumTarihi)
                    .ToListAsync();

                var yorumlarDetayli = new List<object>();
                foreach (var yorum in yorumlar)
                {
                    var kullanici = await _userManager.FindByIdAsync(yorum.KullaniciId);
                    
                    yorumlarDetayli.Add(new
                    {
                        id = yorum.Id,
                        urunId = yorum.UrunId,
                        urunAd = yorum.Urun?.Ad ?? "Bilinmiyor",
                        urunGorselUrl = yorum.Urun?.GorselUrl ?? "",
                        kullaniciEmail = kullanici?.Email ?? "Bilinmiyor",
                        kullaniciAdSoyad = kullanici?.AdSoyad ?? "Bilinmiyor",
                        metin = yorum.Metin,
                        puan = yorum.Puan,
                        yorumTarihi = yorum.YorumTarihi,
                        isimGosterilsin = yorum.IsimGosterilsin,
                        onaylandi = yorum.Onaylandi
                    });
                }

                _logger.LogInformation("Admin tarafından {YorumSayisi} yorum getirildi", yorumlarDetayli.Count);
                return Ok(yorumlarDetayli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm yorumlar getirilirken hata oluştu");
                return StatusCode(500, new { mesaj = "Yorumlar getirilirken hata oluştu.", hata = ex.Message });
            }
        }

        /// <summary>
        /// Onay bekleyen tüm yorumları getirir (Sadece Admin)
        /// </summary>
        [HttpGet("onay-bekleyenler")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> OnayBekleyenYorumlar()
        {
            try
            {
                var yorumlar = await _context.Yorumlar
                    .Include(y => y.Urun)
                    .Where(y => y.Onaylandi == false)
                    .OrderByDescending(y => y.YorumTarihi)
                    .ToListAsync();

                var yorumlarDetayli = new List<object>();
                foreach (var yorum in yorumlar)
                {
                    var kullanici = await _userManager.FindByIdAsync(yorum.KullaniciId);
                    
                    yorumlarDetayli.Add(new
                    {
                        id = yorum.Id,
                        urunId = yorum.UrunId,
                        urunAd = yorum.Urun?.Ad ?? "Bilinmiyor",
                        kullaniciEmail = kullanici?.Email ?? "Bilinmiyor",
                        kullaniciAdSoyad = kullanici?.AdSoyad ?? "Bilinmiyor",
                        metin = yorum.Metin,
                        puan = yorum.Puan,
                        yorumTarihi = yorum.YorumTarihi,
                        isimGosterilsin = yorum.IsimGosterilsin
                    });
                }

                _logger.LogInformation("Admin tarafından {YorumSayisi} onay bekleyen yorum getirildi", yorumlarDetayli.Count);
                return Ok(yorumlarDetayli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Onay bekleyen yorumlar getirilirken hata oluştu");
                return StatusCode(500, new { mesaj = "Yorumlar getirilirken hata oluştu.", hata = ex.Message });
            }
        }

        /// <summary>
        /// Yorumu onayla (Sadece Admin)
        /// </summary>
        [HttpPut("{id}/onayla")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> YorumuOnayla(int id)
        {
            try
            {
                var yorum = await _context.Yorumlar.FindAsync(id);
                
                if (yorum == null)
                {
                    return NotFound(new { mesaj = $"ID'si {id} olan yorum bulunamadı." });
                }

                yorum.Onaylandi = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Yorum {YorumId} admin tarafından onaylandı", id);
                return Ok(new { mesaj = "Yorum başarıyla onaylandı!", yorumId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yorum {YorumId} onaylanırken hata oluştu", id);
                return StatusCode(500, new { mesaj = "Yorum onaylanırken hata oluştu.", hata = ex.Message });
            }
        }

        /// <summary>
        /// Yorumu sil (Sadece Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> YorumuSil(int id)
        {
            try
            {
                var yorum = await _context.Yorumlar.FindAsync(id);
                
                if (yorum == null)
                {
                    return NotFound(new { mesaj = $"ID'si {id} olan yorum bulunamadı." });
                }

                _context.Yorumlar.Remove(yorum);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Yorum {YorumId} admin tarafından silindi", id);
                return Ok(new { mesaj = "Yorum başarıyla silindi!", yorumId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yorum {YorumId} silinirken hata oluştu", id);
                return StatusCode(500, new { mesaj = "Yorum silinirken hata oluştu.", hata = ex.Message });
            }
        }
    }

    /// <summary>
    /// Yorum ekleme için DTO
    /// </summary>
    public class YorumEkleDto
    {
        public int UrunId { get; set; }
        public string Metin { get; set; } = string.Empty;
        public int Puan { get; set; }
        public bool IsimGosterilsin { get; set; } = true;
    }
}

