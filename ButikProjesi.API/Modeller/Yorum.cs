using System.ComponentModel.DataAnnotations;

namespace ButikProjesi.API.Modeller
{
    /// <summary>
    /// Ürün yorumu bilgilerini tutan model sınıfı
    /// </summary>
    public class Yorum
    {
        /// <summary>
        /// Yorum benzersiz kimlik numarası
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Yorum yapılan ürünün kimlik numarası
        /// </summary>
        [Required(ErrorMessage = "Ürün ID'si zorunludur")]
        public int UrunId { get; set; }

        /// <summary>
        /// Yorumu yapan kullanıcının kimlik numarası
        /// </summary>
        [Required(ErrorMessage = "Kullanıcı ID'si zorunludur")]
        public string KullaniciId { get; set; } = string.Empty;

        /// <summary>
        /// Yorum metni
        /// </summary>
        [Required(ErrorMessage = "Yorum metni zorunludur")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Yorum 10-1000 karakter arasında olmalıdır")]
        public string Metin { get; set; } = string.Empty;

        /// <summary>
        /// Verilen puan (1-5 yıldız)
        /// </summary>
        [Required(ErrorMessage = "Puan zorunludur")]
        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır")]
        public int Puan { get; set; }

        /// <summary>
        /// Yorum yapılma tarihi
        /// </summary>
        [Required]
        public DateTime YorumTarihi { get; set; } = DateTime.Now;

        /// <summary>
        /// Kullanıcının adı ve soyadı yorumda görünsün mü?
        /// True: Tam ad gösterilir (örn: "Fırat Engin")
        /// False: Sadece baş harfler gösterilir (örn: "F. E.")
        /// </summary>
        public bool IsimGosterilsin { get; set; } = true;

        /// <summary>
        /// Yorumun admin tarafından onaylanma durumu
        /// True: Yorum onaylandı ve herkese açık gösteriliyor
        /// False: Yorum onay bekliyor (varsayılan)
        /// </summary>
        public bool Onaylandi { get; set; } = false;

        /// <summary>
        /// İlgili ürün (Navigation Property)
        /// </summary>
        public virtual Urun? Urun { get; set; }
    }
}

