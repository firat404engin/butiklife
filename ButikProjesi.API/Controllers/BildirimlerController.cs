using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ButikProjesi.API.Modeller;
using System.Security.Claims;

namespace ButikProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BildirimlerController : ControllerBase
    {
        private readonly VeriTabaniContext _context;

        public BildirimlerController(VeriTabaniContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Test için manuel bildirim oluşturur
        /// </summary>
        [HttpPost("test-bildirim")]
        [Authorize] // Sadece giriş yapmış kullanıcılar için
        public async Task<ActionResult> TestBildirim()
        {
            var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(kullaniciId))
                return Unauthorized();

            var testBildirim = new Bildirim
            {
                KullaniciId = kullaniciId,
                UrunId = 1, // İlk ürün
                Mesaj = "Test bildirimi - Favori ürününüz indirime girdi!",
                OkunduMu = false,
                OlusturmaTarihi = DateTime.UtcNow
            };

            _context.Bildirimler.Add(testBildirim);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Test bildirimi oluşturuldu" });
        }

        /// <summary>
        /// Kullanıcının okunmamış bildirimlerini getirir
        /// </summary>
        [HttpGet]
        [Authorize] // Sadece giriş yapmış kullanıcılar için
        public async Task<ActionResult<IEnumerable<Bildirim>>> GetBildirimler()
        {
            var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(kullaniciId))
                return Unauthorized();

            var bildirimler = await _context.Bildirimler
                .Include(b => b.Urun)
                .Where(b => b.KullaniciId == kullaniciId && !b.OkunduMu)
                .OrderByDescending(b => b.OlusturmaTarihi)
                .ToListAsync();

            return Ok(bildirimler);
        }

        /// <summary>
        /// Akıllı fiyat düşüşü kontrolü yapar ve bildirim oluşturur
        /// </summary>
        [HttpPost("kontrol-et")]
        [Authorize] // Sadece giriş yapmış kullanıcılar için
        public async Task<ActionResult> KontrolEt()
        {
            var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(kullaniciId))
                return Unauthorized();

            try
            {
                // Kullanıcının favorilerini ürün bilgileriyle birlikte getir
                var favoriler = await _context.Favoriler
                    .Include(f => f.Urun) // İlişkili ürün bilgisini de yükle
                    .Where(f => f.KullaniciId == kullaniciId)
                    .ToListAsync();

                if (!favoriler.Any())
                    return Ok(new { message = "Favori ürün bulunamadı" });

                var yeniBildirimler = new List<Bildirim>();

                foreach (var favori in favoriler)
                {
                    // Fiyat Düşüşü Kontrolü: Güncel fiyat, eklendiği andaki fiyattan düşük mü?
                    if (favori.Urun.Fiyat < favori.FiyatEklendiginde)
                    {
                        // Yeni fiyat metnini oluştur
                        var yeniFiyatMetni = favori.Urun.Fiyat.ToString("C"); // Örn: "₺2.400,00"
                        
                        // Bu fiyat düşüşü için daha önce bildirim gönderilmiş mi diye kontrol et
                        // Hem ürün ve kullanıcıyı hem de mesajda yeni fiyatı kontrol et
                        var mevcutBildirim = await _context.Bildirimler
                            .FirstOrDefaultAsync(b => b.KullaniciId == kullaniciId && 
                                                       b.UrunId == favori.UrunId && 
                                                       b.Mesaj.Contains(yeniFiyatMetni)); // Mesajda yeni fiyat var mı?

                        if (mevcutBildirim == null)
                        {
                            // İndirim oranını hesapla
                            var indirimOrani = 0;
                            if (favori.FiyatEklendiginde > 0) 
                            {
                                indirimOrani = (int)Math.Round(((favori.FiyatEklendiginde - favori.Urun.Fiyat) / favori.FiyatEklendiginde) * 100);
                            }
                            
                            // Bildirim yoksa, yenisini oluştur
                            var yeniBildirim = new Bildirim
                            {
                                KullaniciId = kullaniciId,
                                UrunId = favori.UrunId,
                                Mesaj = $"Favoriniz '{favori.Urun.Ad}', %{indirimOrani} indirime girdi!",
                                OkunduMu = false,
                                OlusturmaTarihi = DateTime.UtcNow
                            };
                            _context.Bildirimler.Add(yeniBildirim);
                            yeniBildirimler.Add(yeniBildirim);
                        }
                    }
                }

                if (yeniBildirimler.Any())
                {
                    await _context.SaveChangesAsync();
                }

                return Ok(new { 
                    message = $"{yeniBildirimler.Count} yeni bildirim oluşturuldu",
                    bildirimSayisi = yeniBildirimler.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Bildirim kontrolü sırasında hata oluştu", error = ex.Message });
            }
        }

        /// <summary>
        /// Kullanıcının tüm bildirimlerini okundu olarak işaretler
        /// </summary>
        [HttpPut("hepsini-okundu-isaretle")]
        [Authorize] // Sadece giriş yapmış kullanıcılar için
        public async Task<ActionResult> HepsiniOkunduIsaretle()
        {
            var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(kullaniciId))
                return Unauthorized();

            try
            {
                var bildirimler = await _context.Bildirimler
                    .Where(b => b.KullaniciId == kullaniciId && !b.OkunduMu)
                    .ToListAsync();

                foreach (var bildirim in bildirimler)
                {
                    bildirim.OkunduMu = true;
                }

                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = $"{bildirimler.Count} bildirim okundu olarak işaretlendi",
                    okunanBildirimSayisi = bildirimler.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Bildirimler güncellenirken hata oluştu", error = ex.Message });
            }
        }

        /// <summary>
        /// Belirli bir bildirimi okundu olarak işaretler
        /// </summary>
        [HttpPut("{id}/okundu-isaretle")]
        [Authorize] // Sadece giriş yapmış kullanıcılar için
        public async Task<ActionResult> OkunduIsaretle(int id)
        {
            var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(kullaniciId))
                return Unauthorized();

            try
            {
                var bildirim = await _context.Bildirimler
                    .FirstOrDefaultAsync(b => b.Id == id && b.KullaniciId == kullaniciId);

                if (bildirim == null)
                    return NotFound(new { message = "Bildirim bulunamadı" });

                bildirim.OkunduMu = true;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Bildirim okundu olarak işaretlendi" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Bildirim güncellenirken hata oluştu", error = ex.Message });
            }
        }

        /// <summary>
        /// Tüm bildirimleri temizler (Sadece Admin)
        /// </summary>
        [HttpPost("temizle")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TumBildirimleriTemizle()
        {
            try
            {
                var tumBildirimler = await _context.Bildirimler.ToListAsync();
                _context.Bildirimler.RemoveRange(tumBildirimler);
                await _context.SaveChangesAsync();
                
                return Ok(new { message = $"Tüm bildirimler silindi. ({tumBildirimler.Count} bildirim temizlendi)" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Bildirimler temizlenirken hata oluştu", error = ex.Message });
            }
        }
    }
}
