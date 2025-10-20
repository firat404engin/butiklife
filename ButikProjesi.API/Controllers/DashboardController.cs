using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ButikProjesi.API.Modeller;

namespace ButikProjesi.API.Controllers
{
    /// <summary>
    /// Admin dashboard istatistikleri için API controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly VeriTabaniContext _veriTabaniContext;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            VeriTabaniContext veriTabaniContext,
            ILogger<DashboardController> logger)
        {
            _veriTabaniContext = veriTabaniContext;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard istatistiklerini getirir (toplam ürün, sipariş, kullanıcı sayısı)
        /// </summary>
        /// <returns>Dashboard istatistikleri</returns>
        [HttpGet("stats")]
        public async Task<ActionResult> IstatistikleriGetir()
        {
            try
            {
                _logger.LogInformation("Dashboard istatistikleri istendi");

                // Toplam ürün sayısı
                var toplamUrun = await _veriTabaniContext.Urunler.CountAsync();

                // Toplam sipariş sayısı
                var toplamSiparis = await _veriTabaniContext.Siparisler.CountAsync();

                // Toplam kullanıcı sayısı (AspNetUsers tablosundan)
                var toplamKullanici = await _veriTabaniContext.Users.CountAsync();

                // Toplam gelir (tüm siparişlerin toplamı)
                var toplamGelir = await _veriTabaniContext.Siparisler
                    .SumAsync(s => s.ToplamTutar);

                // Bekleyen sipariş sayısı
                var bekleyenSiparis = await _veriTabaniContext.Siparisler
                    .CountAsync(s => s.Durum == "Beklemede" || s.Durum == "Hazırlanıyor");

                // Düşük stoklu ürün sayısı (10'dan az)
                var dusukStokluUrun = await _veriTabaniContext.Urunler
                    .CountAsync(u => u.StokAdedi < 10);

                _logger.LogInformation(
                    "İstatistikler: Ürün={Urun}, Sipariş={Siparis}, Kullanıcı={Kullanici}",
                    toplamUrun, toplamSiparis, toplamKullanici);

                return Ok(new
                {
                    toplamUrun,
                    toplamSiparis,
                    toplamKullanici,
                    toplamGelir,
                    bekleyenSiparis,
                    dusukStokluUrun
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard istatistikleri alınırken hata oluştu");
                return StatusCode(500, new
                {
                    hata = "İstatistikler alınırken bir hata oluştu",
                    detay = ex.Message
                });
            }
        }
    }
}

