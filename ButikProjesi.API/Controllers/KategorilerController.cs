using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ButikProjesi.API.Modeller;
using Microsoft.AspNetCore.Authorization;

namespace ButikProjesi.API.Controllers
{
    /// <summary>
    /// Kategoriler ile ilgili API işlemlerini yöneten controller sınıfı
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class KategorilerController : ControllerBase
    {
        private readonly VeriTabaniContext _veriTabaniContext;

        /// <summary>
        /// KategorilerController constructor
        /// </summary>
        /// <param name="veriTabaniContext">Veritabanı bağlam sınıfı</param>
        public KategorilerController(VeriTabaniContext veriTabaniContext)
        {
            _veriTabaniContext = veriTabaniContext;
        }

        /// <summary>
        /// Tüm kategorilerin listesini getirir
        /// </summary>
        /// <returns>Kategori listesi</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Kategori>>> TumKategorileriGetir()
        {
            try
            {
                // Veritabanından tüm kategorileri getir
                var kategoriler = await _veriTabaniContext.Kategoriler
                    .ToListAsync();

                return Ok(kategoriler);
            }
            catch (Exception ex)
            {
                // Hata durumunda 500 Internal Server Error döndür
                return StatusCode(500, new { 
                    Hata = "Kategoriler getirilirken bir hata oluştu", 
                    Detay = ex.Message 
                });
            }
        }

        /// <summary>
        /// Yeni kategori oluşturur (Sadece Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Kategori>> KategoriOlustur([FromBody] Kategori kategori)
        {
            if (kategori == null || string.IsNullOrWhiteSpace(kategori.Ad))
            {
                return BadRequest(new { Hata = "Kategori adı zorunludur" });
            }

            _veriTabaniContext.Kategoriler.Add(kategori);
            await _veriTabaniContext.SaveChangesAsync();
            return CreatedAtAction(nameof(KategoriGetir), new { id = kategori.Id }, kategori);
        }

        /// <summary>
        /// Kategori bilgilerini günceller (Sadece Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Kategori>> KategoriGuncelle(int id, [FromBody] Kategori kategori)
        {
            var mevcut = await _veriTabaniContext.Kategoriler.FindAsync(id);
            if (mevcut == null)
            {
                return NotFound(new { Hata = $"ID'si {id} olan kategori bulunamadı" });
            }

            if (kategori == null || string.IsNullOrWhiteSpace(kategori.Ad))
            {
                return BadRequest(new { Hata = "Kategori adı zorunludur" });
            }

            mevcut.Ad = kategori.Ad;
            await _veriTabaniContext.SaveChangesAsync();
            return Ok(mevcut);
        }

        /// <summary>
        /// Kategoriyi siler (Sadece Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> KategoriSil(int id)
        {
            var mevcut = await _veriTabaniContext.Kategoriler.FindAsync(id);
            if (mevcut == null)
            {
                return NotFound(new { Hata = $"ID'si {id} olan kategori bulunamadı" });
            }

            _veriTabaniContext.Kategoriler.Remove(mevcut);
            await _veriTabaniContext.SaveChangesAsync();
            return Ok(new { Mesaj = "Kategori silindi" });
        }

        /// <summary>
        /// Belirtilen ID'ye sahip kategoriyi getirir
        /// </summary>
        /// <param name="id">Kategori kimlik numarası</param>
        /// <returns>Kategori bilgisi</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Kategori>> KategoriGetir(int id)
        {
            try
            {
                // Veritabanından belirtilen ID'ye sahip kategoriyi getir
                var kategori = await _veriTabaniContext.Kategoriler
                    .FirstOrDefaultAsync(k => k.Id == id);

                if (kategori == null)
                {
                    // Kategori bulunamadığında 404 Not Found döndür
                    return NotFound(new { 
                        Hata = $"ID'si {id} olan kategori bulunamadı" 
                    });
                }

                return Ok(kategori);
            }
            catch (Exception ex)
            {
                // Hata durumunda 500 Internal Server Error döndür
                return StatusCode(500, new { 
                    Hata = "Kategori getirilirken bir hata oluştu", 
                    Detay = ex.Message 
                });
            }
        }

        /// <summary>
        /// Belirtilen kategoriye ait ürünleri getirir
        /// </summary>
        /// <param name="id">Kategori kimlik numarası</param>
        /// <returns>Kategoriye ait ürün listesi</returns>
        [HttpGet("{id}/urunler")]
        public async Task<ActionResult<IEnumerable<Urun>>> KategoriUrunleriniGetir(int id)
        {
            try
            {
                // Önce kategori var mı kontrol et
                var kategori = await _veriTabaniContext.Kategoriler
                    .FirstOrDefaultAsync(k => k.Id == id);

                if (kategori == null)
                {
                    return NotFound(new { 
                        Hata = $"ID'si {id} olan kategori bulunamadı" 
                    });
                }

                // Kategoriye ait ürünleri getir
                var urunler = await _veriTabaniContext.Urunler
                    .Where(u => u.KategoriId == id)
                    .ToListAsync();

                return Ok(urunler);
            }
            catch (Exception ex)
            {
                // Hata durumunda 500 Internal Server Error döndür
                return StatusCode(500, new { 
                    Hata = "Kategori ürünleri getirilirken bir hata oluştu", 
                    Detay = ex.Message 
                });
            }
        }
    }
}
