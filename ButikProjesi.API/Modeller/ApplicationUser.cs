using Microsoft.AspNetCore.Identity;

namespace ButikProjesi.API.Modeller
{
    /// <summary>
    /// Genişletilmiş kullanıcı modeli - IdentityUser'dan miras alır
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Kullanıcının adı ve soyadı
        /// </summary>
        public string? AdSoyad { get; set; }

        /// <summary>
        /// Kullanıcının adresi
        /// </summary>
        public string? Adres { get; set; }

        /// <summary>
        /// Kullanıcının telefon numarası (ek alan - Email ile karışmaması için)
        /// </summary>
        public string? TelefonNumarasi { get; set; }
    }
}











