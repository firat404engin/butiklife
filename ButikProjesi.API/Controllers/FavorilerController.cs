using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ButikProjesi.API.Modeller;

namespace ButikProjesi.API.Controllers
{
    /// <summary>
    /// Kullanıcı favorileri için API controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavorilerController : ControllerBase
    {
        private readonly VeriTabaniContext _context;
        private readonly ILogger<FavorilerController> _logger;

        public FavorilerController(VeriTabaniContext context, ILogger<FavorilerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Giriş yapmış kullanıcının tüm favori ürünlerini getirir
        /// </summary>
        /// <returns>Favori ürünler listesi</returns>
        [HttpGet]
        public async Task<ActionResult> FavorileriGetir()
        {
            try
            {
                var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(kullaniciId))
                {
                    return Unauthorized(new { mesaj = "Kullanıcı oturumu bulunamadı." });
                }

                var favoriler = await _context.Favoriler
                    .Where(f => f.KullaniciId == kullaniciId)
                    .Include(f => f.Urun)
                        .ThenInclude(u => u!.Kategori)
                    .OrderByDescending(f => f.EklenmeTarihi)
                    .ToListAsync();

                var favoriUrunler = favoriler.Select(f => new
                {
                    favoriId = f.Id,
                    urun = f.Urun,
                    eklenmeTarihi = f.EklenmeTarihi
                }).ToList();

                _logger.LogInformation("Kullanıcı {KullaniciId} için {FavoriSayisi} favori getirildi", kullaniciId, favoriUrunler.Count);
                return Ok(favoriUrunler);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Favoriler getirilirken hata oluştu");
                return StatusCode(500, new { mesaj = "Favoriler getirilirken hata oluştu.", hata = ex.Message });
            }
        }

        /// <summary>
        /// Ürünü kullanıcının favorilerine ekler
        /// </summary>
        /// <param name="urunId">Ürün ID</param>
        /// <returns>İşlem sonucu</returns>
        [HttpPost]
        public async Task<ActionResult> FavoriEkle([FromBody] FavoriEkleDto dto)
        {
            try
            {
                var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(kullaniciId))
                {
                    return Unauthorized(new { mesaj = "Kullanıcı oturumu bulunamadı." });
                }

                // Ürünü getir ve fiyatını al
                var urun = await _context.Urunler.FirstOrDefaultAsync(u => u.Id == dto.UrunId);
                if (urun == null)
                {
                    return NotFound(new { mesaj = $"ID'si {dto.UrunId} olan ürün bulunamadı." });
                }

                // Zaten favorilerde mi kontrol et
                var mevcutFavori = await _context.Favoriler
                    .AnyAsync(f => f.KullaniciId == kullaniciId && f.UrunId == dto.UrunId);

                if (mevcutFavori)
                {
                    return BadRequest(new { mesaj = "Bu ürün zaten favorilerinizde." });
                }

                // Yeni favori oluştur - fiyat snapshot'ı ile birlikte
                var yeniFavori = new Favori
                {
                    KullaniciId = kullaniciId,
                    UrunId = dto.UrunId,
                    EklenmeTarihi = DateTime.Now,
                    FiyatEklendiginde = urun.Fiyat // Fiyat snapshot'ı kaydet
                };

                _context.Favoriler.Add(yeniFavori);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Kullanıcı {KullaniciId} ürün {UrunId}'yi favorilere ekledi", kullaniciId, dto.UrunId);
                return Ok(new { mesaj = "Ürün favorilere eklendi!", favoriId = yeniFavori.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Favori eklenirken hata oluştu");
                return StatusCode(500, new { mesaj = "Favori eklenirken hata oluştu.", hata = ex.Message });
            }
        }

        /// <summary>
        /// Ürünü kullanıcının favorilerinden çıkarır
        /// </summary>
        /// <param name="urunId">Ürün ID</param>
        /// <returns>İşlem sonucu</returns>
        [HttpDelete("{urunId}")]
        public async Task<ActionResult> FavoriSil(int urunId)
        {
            try
            {
                var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(kullaniciId))
                {
                    return Unauthorized(new { mesaj = "Kullanıcı oturumu bulunamadı." });
                }

                var favori = await _context.Favoriler
                    .FirstOrDefaultAsync(f => f.KullaniciId == kullaniciId && f.UrunId == urunId);

                if (favori == null)
                {
                    return NotFound(new { mesaj = "Bu ürün favorilerinizde bulunamadı." });
                }

                _context.Favoriler.Remove(favori);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Kullanıcı {KullaniciId} ürün {UrunId}'yi favorilerden çıkardı", kullaniciId, urunId);
                return Ok(new { mesaj = "Ürün favorilerden çıkarıldı!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Favori silinirken hata oluştu");
                return StatusCode(500, new { mesaj = "Favori silinirken hata oluştu.", hata = ex.Message });
            }
        }

        /// <summary>
        /// Ürünün kullanıcının favorilerinde olup olmadığını kontrol eder
        /// </summary>
        /// <param name="urunId">Ürün ID</param>
        /// <returns>Favoride mi?</returns>
        [HttpGet("kontrol/{urunId}")]
        public async Task<ActionResult> FavoriKontrol(int urunId)
        {
            try
            {
                var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(kullaniciId))
                {
                    return Ok(new { favoride = false });
                }

                var favoride = await _context.Favoriler
                    .AnyAsync(f => f.KullaniciId == kullaniciId && f.UrunId == urunId);

                return Ok(new { favoride = favoride });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Favori kontrolü yapılırken hata oluştu");
                return StatusCode(500, new { mesaj = "Favori kontrolü yapılırken hata oluştu.", hata = ex.Message });
            }
        }
    }

    /// <summary>
    /// Favori ekleme için DTO
    /// </summary>
    public class FavoriEkleDto
    {
        public int UrunId { get; set; }
    }
}










