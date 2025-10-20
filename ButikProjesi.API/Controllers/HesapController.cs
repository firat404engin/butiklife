using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ButikProjesi.API.Modeller;

namespace ButikProjesi.API.Controllers
{
    /// <summary>
    /// Kullanıcı hesap işlemleri için API controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HesapController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<HesapController> _logger;

        public HesapController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<HesapController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Yeni kullanıcı kaydı oluşturur
        /// </summary>
        /// <param name="model">Kayıt bilgileri</param>
        /// <returns>Kayıt sonucu</returns>
        [HttpPost("kayitol")]
        public async Task<ActionResult<AuthYanitDto>> KayitOl([FromBody] KayitDto model)
        {
            try
            {
                _logger.LogInformation("Yeni kayıt isteği alındı: {Email}", model.Email);

                // Model validasyonu
                if (!ModelState.IsValid)
                {
                    var hatalar = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new AuthYanitDto
                    {
                        Basarili = false,
                        Mesaj = "Girilen bilgiler geçersiz",
                        Hatalar = hatalar
                    });
                }

                // E-posta kontrolü
                var mevcutKullanici = await _userManager.FindByEmailAsync(model.Email);
                if (mevcutKullanici != null)
                {
                    _logger.LogWarning("Kayıt başarısız: E-posta zaten kullanımda - {Email}", model.Email);
                    return BadRequest(new AuthYanitDto
                    {
                        Basarili = false,
                        Mesaj = "Bu e-posta adresi zaten kullanılıyor",
                        Hatalar = new List<string> { "Bu e-posta adresi zaten kayıtlı" }
                    });
                }

                // Yeni kullanıcı oluştur - UserName ve Email aynı olmalı
                var user = new ApplicationUser
                {
                    UserName = model.Email,  // Identity, UserName ile çalışır
                    Email = model.Email,      // Email de ayarlanmalı
                    EmailConfirmed = true     // Development için otomatik onaylı
                };

                // Kullanıcıyı şifreyle birlikte oluştur
                var result = await _userManager.CreateAsync(user, model.Sifre);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Yeni kullanıcı başarıyla oluşturuldu: {Email}", model.Email);

                    // Otomatik giriş yap
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return Ok(new AuthYanitDto
                    {
                        Basarili = true,
                        Mesaj = "Kayıt başarılı! Hoş geldiniz.",
                        KullaniciId = user.Id,
                        Email = user.Email
                    });
                }
                else
                {
                    _logger.LogWarning("Kayıt başarısız: {Hatalar}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    return BadRequest(new AuthYanitDto
                    {
                        Basarili = false,
                        Mesaj = "Kayıt işlemi başarısız oldu",
                        Hatalar = result.Errors.Select(e => e.Description).ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kayıt işlemi sırasında hata oluştu");
                return StatusCode(500, new AuthYanitDto
                {
                    Basarili = false,
                    Mesaj = "Kayıt işlemi sırasında bir hata oluştu",
                    Hatalar = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Kullanıcı girişi yapar
        /// </summary>
        /// <param name="model">Giriş bilgileri</param>
        /// <returns>Giriş sonucu</returns>
        [HttpPost("girisyap")]
        public async Task<ActionResult<AuthYanitDto>> GirisYap([FromBody] GirisDto model)
        {
            try
            {
                _logger.LogInformation("Giriş isteği alındı: {Email}", model.Email);

                // Model validasyonu
                if (!ModelState.IsValid)
                {
                    var hatalar = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new AuthYanitDto
                    {
                        Basarili = false,
                        Mesaj = "Girilen bilgiler geçersiz",
                        Hatalar = hatalar
                    });
                }

                // Kullanıcıyı e-posta ile bul
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || string.IsNullOrEmpty(user.UserName))
                {
                    _logger.LogWarning("Giriş başarısız: Kullanıcı bulunamadı - {Email}", model.Email);
                    return Unauthorized(new AuthYanitDto
                    {
                        Basarili = false,
                        Mesaj = "Kullanıcı bulunamadı veya şifre hatalı",
                        Hatalar = new List<string> { "Girdiğiniz bilgiler hatalı" }
                    });
                }

                // Identity, UserName ile çalışır - PasswordSignInAsync hem kontrol eder hem giriş yapar
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName,           // UserName kullan (Email ile aynı)
                    model.Sifre,             // Şifre
                    model.BeniHatirla,       // Beni hatırla
                    lockoutOnFailure: false  // Hesap kilitleme kapalı
                );

                if (result.Succeeded)
                {
                    _logger.LogInformation("Giriş başarılı: {Email}", model.Email);
                    return Ok(new AuthYanitDto
                    {
                        Basarili = true,
                        Mesaj = "Giriş başarılı! Hoş geldiniz.",
                        KullaniciId = user.Id,
                        Email = user.Email
                    });
                }
                else
                {
                    _logger.LogWarning("Giriş başarısız: Şifre hatalı - {Email}", model.Email);
                    return Unauthorized(new AuthYanitDto
                    {
                        Basarili = false,
                        Mesaj = "Kullanıcı bulunamadı veya şifre hatalı",
                        Hatalar = new List<string> { "Girdiğiniz bilgiler hatalı" }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Giriş işlemi sırasında hata oluştu");
                return StatusCode(500, new AuthYanitDto
                {
                    Basarili = false,
                    Mesaj = "Giriş işlemi sırasında bir hata oluştu",
                    Hatalar = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Giriş yapmış kullanıcının bilgilerini ve rollerini döndürür (Authentication durumu kontrolü için)
        /// </summary>
        /// <returns>Kullanıcı bilgileri ve rolleri veya 401</returns>
        [Authorize]
        [HttpGet("kullanici-bilgisi")]
        public async Task<ActionResult> KullaniciBilgisiGetir()
        {
            try
            {
                // 1. O anki isteği yapan kullanıcıyı bul
                var kullanici = await _userManager.GetUserAsync(User);
                
                if (kullanici == null)
                {
                    _logger.LogWarning("Kullanıcı bilgisi alınamadı: Kullanıcı bulunamadı");
                    return Unauthorized(new { basarili = false, mesaj = "Kullanıcı oturumu bulunamadı" });
                }

                // 2. Kullanıcının rollerini getir
                var roller = await _userManager.GetRolesAsync(kullanici);
                
                _logger.LogInformation("Kullanıcı bilgileri alındı: {Email}, Roller: {Roller}", 
                    kullanici.Email, string.Join(", ", roller));

                // 3. Temel claim'leri oluştur
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, kullanici.Id),
                    new Claim(ClaimTypes.Name, kullanici.Email ?? ""),
                    new Claim(ClaimTypes.Email, kullanici.Email ?? "")
                };

                // 4. Her bir rol için claim ekle
                foreach (var rol in roller)
                {
                    claims.Add(new Claim(ClaimTypes.Role, rol));
                }

                // 5. Claim'leri frontend'e dön
                return Ok(new
                {
                    basarili = true,
                    mesaj = "Kullanıcı bilgileri başarıyla alındı",
                    kullaniciId = kullanici.Id,
                    email = kullanici.Email,
                    claims = claims.Select(c => new { type = c.Type, value = c.Value }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı bilgileri alınırken hata oluştu");
                return Unauthorized(new { basarili = false, mesaj = "Kullanıcı bilgileri alınamadı" });
            }
        }

        /// <summary>
        /// Kullanıcı çıkışı yapar
        /// </summary>
        /// <returns>Çıkış sonucu</returns>
        [HttpPost("cikisyap")]
        public async Task<ActionResult<AuthYanitDto>> CikisYap()
        {
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation("Kullanıcı çıkış yaptı");

                return Ok(new AuthYanitDto
                {
                    Basarili = true,
                    Mesaj = "Başarıyla çıkış yaptınız"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Çıkış işlemi sırasında hata oluştu");
                return StatusCode(500, new AuthYanitDto
                {
                    Basarili = false,
                    Mesaj = "Çıkış işlemi sırasında bir hata oluştu",
                    Hatalar = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Mevcut kullanıcı bilgilerini getirir
        /// </summary>
        /// <returns>Kullanıcı bilgileri</returns>
        [HttpGet("kullanici")]
        public async Task<ActionResult<AuthYanitDto>> KullaniciGetir()
        {
            try
            {
                var kullanici = await _userManager.GetUserAsync(User);
                if (kullanici == null)
                {
                    return Ok(new AuthYanitDto
                    {
                        Basarili = false,
                        Mesaj = "Kullanıcı oturumu bulunamadı"
                    });
                }

                return Ok(new AuthYanitDto
                {
                    Basarili = true,
                    Mesaj = "Kullanıcı bilgileri getirildi",
                    KullaniciId = kullanici.Id,
                    Email = kullanici.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı bilgileri getirilirken hata oluştu");
                return StatusCode(500, new AuthYanitDto
                {
                    Basarili = false,
                    Mesaj = "Kullanıcı bilgileri getirilirken hata oluştu",
                    Hatalar = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Kullanıcının profil bilgilerini getirir (AdSoyad, Adres, Telefon)
        /// </summary>
        /// <returns>Profil bilgileri</returns>
        [Authorize]
        [HttpGet("profil")]
        public async Task<ActionResult> ProfilGetir()
        {
            try
            {
                var kullanici = await _userManager.GetUserAsync(User);
                
                if (kullanici == null)
                {
                    return Unauthorized(new { basarili = false, mesaj = "Kullanıcı oturumu bulunamadı" });
                }

                return Ok(new
                {
                    basarili = true,
                    email = kullanici.Email,
                    adSoyad = kullanici.AdSoyad,
                    adres = kullanici.Adres,
                    telefonNumarasi = kullanici.TelefonNumarasi
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil bilgileri getirilirken hata oluştu");
                return StatusCode(500, new { basarili = false, mesaj = "Profil bilgileri getirilirken hata oluştu.", hata = ex.Message });
            }
        }

        /// <summary>
        /// Tüm müşterileri getirir (Sadece Admin)
        /// </summary>
        /// <returns>Müşteri listesi</returns>
        [HttpGet("musteriler")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> MusterileriGetir()
        {
            try
            {
                var musteriler = _userManager.Users
                    .OrderByDescending(u => u.Id)
                    .ToList();

                var musteriListesi = musteriler.Select(m => new
                {
                    id = m.Id,
                    email = m.Email,
                    adSoyad = m.AdSoyad ?? "Belirtilmemiş",
                    telefon = m.TelefonNumarasi ?? "Belirtilmemiş",
                    adres = m.Adres ?? "Belirtilmemiş",
                    kayitTarihi = m.Id, // ID'den yaklaşık tarih çıkarabiliriz
                    toplamSiparis = 0 // Bu bilgiyi ayrı bir sorgu ile alabiliriz
                }).ToList();

                _logger.LogInformation("Admin tarafından {MusteriSayisi} müşteri getirildi", musteriListesi.Count);
                return Ok(musteriListesi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteriler getirilirken hata oluştu");
                return StatusCode(500, new { mesaj = "Müşteriler getirilirken hata oluştu.", hata = ex.Message });
            }
        }

        /// <summary>
        /// Kullanıcının profil bilgilerini günceller
        /// </summary>
        /// <param name="profilDto">Güncellenecek profil bilgileri</param>
        /// <returns>Güncelleme sonucu</returns>
        [Authorize]
        [HttpPut("profil")]
        public async Task<ActionResult> ProfilGuncelle([FromBody] ProfilGuncelleDto profilDto)
        {
            try
            {
                var kullanici = await _userManager.GetUserAsync(User);
                
                if (kullanici == null)
                {
                    return Unauthorized(new { basarili = false, mesaj = "Kullanıcı oturumu bulunamadı" });
                }

                // Profil bilgilerini güncelle
                kullanici.AdSoyad = profilDto.AdSoyad;
                kullanici.Adres = profilDto.Adres;
                kullanici.TelefonNumarasi = profilDto.TelefonNumarasi;

                var sonuc = await _userManager.UpdateAsync(kullanici);

                if (sonuc.Succeeded)
                {
                    _logger.LogInformation("Kullanıcı profili güncellendi: {Email}", kullanici.Email);
                    return Ok(new
                    {
                        basarili = true,
                        mesaj = "Profil bilgileriniz başarıyla güncellendi!"
                    });
                }
                else
                {
                    var hatalar = sonuc.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("Profil güncelleme başarısız: {Hatalar}", string.Join(", ", hatalar));
                    return BadRequest(new
                    {
                        basarili = false,
                        mesaj = "Profil güncellenirken hata oluştu",
                        hatalar = hatalar
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil güncellenirken hata oluştu");
                return StatusCode(500, new { basarili = false, mesaj = "Profil güncellenirken hata oluştu.", hata = ex.Message });
            }
        }
    }

    /// <summary>
    /// Profil güncelleme için DTO
    /// </summary>
    public class ProfilGuncelleDto
    {
        public string? AdSoyad { get; set; }
        public string? Adres { get; set; }
        public string? TelefonNumarasi { get; set; }
    }
}

