using System.ComponentModel.DataAnnotations;

namespace ButikProjesi.Istemci.Modeller
{
    /// <summary>
    /// Kullanıcı kayıt işlemi için DTO
    /// </summary>
    public class KayitDto
    {
        /// <summary>
        /// E-posta adresi
        /// </summary>
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Şifre
        /// </summary>
        [Required(ErrorMessage = "Şifre zorunludur")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        public string Sifre { get; set; } = string.Empty;

        /// <summary>
        /// Şifre tekrarı
        /// </summary>
        [Required(ErrorMessage = "Şifre tekrarı zorunludur")]
        [Compare("Sifre", ErrorMessage = "Şifreler eşleşmiyor")]
        public string SifreTekrar { get; set; } = string.Empty;
    }

    /// <summary>
    /// Kullanıcı giriş işlemi için DTO
    /// </summary>
    public class GirisDto
    {
        /// <summary>
        /// E-posta adresi
        /// </summary>
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Şifre
        /// </summary>
        [Required(ErrorMessage = "Şifre zorunludur")]
        public string Sifre { get; set; } = string.Empty;

        /// <summary>
        /// Beni hatırla
        /// </summary>
        public bool BeniHatirla { get; set; } = false;
    }

    /// <summary>
    /// Authentication yanıt DTO
    /// </summary>
    public class AuthYanitDto
    {
        /// <summary>
        /// İşlem başarılı mı
        /// </summary>
        public bool Basarili { get; set; }

        /// <summary>
        /// Mesaj
        /// </summary>
        public string Mesaj { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcı ID
        /// </summary>
        public string? KullaniciId { get; set; }

        /// <summary>
        /// E-posta
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Hata mesajları listesi
        /// </summary>
        public List<string> Hatalar { get; set; } = new();
    }
}











