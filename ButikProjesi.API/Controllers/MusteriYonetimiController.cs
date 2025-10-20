using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ButikProjesi.API.Modeller;
using ButikProjesi.Shared.Modeller;

namespace ButikProjesi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class MusteriYonetimiController : ControllerBase
    {
        private readonly VeriTabaniContext _context;

        public MusteriYonetimiController(VeriTabaniContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<MusteriDto>>> MusterileriGetir()
        {
            try
            {
                var musteriler = await _context.Users
                    .Select(u => new MusteriDto
                    {
                        Id = u.Id,
                        Email = u.Email ?? string.Empty,
                        AdSoyad = u.AdSoyad ?? "Belirtilmemiş",
                        SiparisAdeti = _context.Siparisler.Count(s => s.KullaniciId == u.Id),
                        KayitTarihi = DateTime.Now // Geçici olarak bugünün tarihi
                    })
                    .OrderByDescending(m => m.SiparisAdeti)
                    .ToListAsync();

                return Ok(musteriler);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Müşteriler getirilirken hata oluştu: {ex.Message}");
            }
        }

        [HttpGet("{kullaniciId}/siparisler")]
        public async Task<ActionResult<List<SiparisDto>>> MusterininSiparisleriniGetir(string kullaniciId)
        {
            try
            {
                var siparisler = await _context.Siparisler
                    .Where(s => s.KullaniciId == kullaniciId)
                    .Select(s => new SiparisDto
                    {
                        Id = s.Id,
                        SiparisTarihi = s.SiparisTarihi,
                        ToplamTutar = s.ToplamTutar,
                        Durum = s.Durum,
                        Adres = s.Adres,
                        Telefon = s.Telefon,
                        Notlar = s.SiparisNotu
                    })
                    .OrderByDescending(s => s.SiparisTarihi)
                    .ToListAsync();

                return Ok(siparisler);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Müşteri siparişleri getirilirken hata oluştu: {ex.Message}");
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
