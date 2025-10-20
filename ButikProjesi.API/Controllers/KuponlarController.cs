using ButikProjesi.API.Modeller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class KuponlarController : ControllerBase
{
    private readonly VeriTabaniContext _context;
    private readonly ILogger<KuponlarController> _logger;

    public KuponlarController(VeriTabaniContext context, ILogger<KuponlarController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("dogrula/{kod}")]
    public async Task<ActionResult<Kupon>> KuponDogrula(string kod)
    {
        _logger.LogInformation("Kupon doğrulanıyor: {Kod}", kod);
        if (string.IsNullOrWhiteSpace(kod))
        {
            return BadRequest("Kupon kodu boş olamaz.");
        }

        var kupon = await _context.Kuponlar.FirstOrDefaultAsync(k => k.Kod.ToUpper() == kod.ToUpper());

        if (kupon == null)
        {
            _logger.LogWarning("Kod '{Kod}' ile kupon veritabanında bulunamadı.", kod);
            return NotFound("Geçersiz kupon");
        }

        if (!kupon.AktifMi)
        {
            _logger.LogWarning("Kod '{Kod}' ile kupon bulundu ancak durumu AKTİF DEĞİL. Durum: {AktifMi}", kod, kupon.AktifMi);
            return NotFound("Geçersiz kupon");
        }

        if (kupon.SonGecerlilikTarihi.HasValue && kupon.SonGecerlilikTarihi.Value.Date < DateTime.UtcNow.Date)
        {
            _logger.LogWarning("Kod '{Kod}' ile kupon bulundu ancak SÜRESİ DOLMUŞ. Tarih: {Tarih}", kod, kupon.SonGecerlilikTarihi.Value);
            return NotFound("Geçersiz kupon");
        }

        _logger.LogInformation("Kod '{Kod}' ile kupon başarıyla doğrulandı.", kod);
        return Ok(kupon);
    }

    // Diğer Admin metotları...
    [HttpGet, Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<Kupon>>> TumKuponlariGetir()
    {
        return Ok(await _context.Kuponlar.ToListAsync());
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<ActionResult<Kupon>> KuponOlustur(Kupon kupon)
    {
        _context.Kuponlar.Add(kupon);
        await _context.SaveChangesAsync();
        return Ok(kupon);
    }

    [HttpPut("{id}/toggle-aktif", Name = "ToggleAktif"), Authorize(Roles = "Admin")]
    public async Task<ActionResult> ToggleAktif(int id)
    {
        var kupon = await _context.Kuponlar.FindAsync(id);
        if (kupon == null)
        {
            return NotFound();
        }

        _logger.LogInformation("Kupon durumu değiştiriliyor - ID: {Id}, Eski Durum: {EskiDurum}", id, kupon.AktifMi);
        
        kupon.AktifMi = !kupon.AktifMi;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Kupon durumu güncellendi - ID: {Id}, Yeni Durum: {YeniDurum}", id, kupon.AktifMi);
        
        return Ok(new { AktifMi = kupon.AktifMi });
    }

    [HttpDelete("{id}", Name = "KuponSil"), Authorize(Roles = "Admin")]
    public async Task<ActionResult> KuponSil(int id)
    {
        var kupon = await _context.Kuponlar.FindAsync(id);
        if (kupon == null)
        {
            return NotFound();
        }

        _context.Kuponlar.Remove(kupon);
        await _context.SaveChangesAsync();
        return Ok();
    }
}