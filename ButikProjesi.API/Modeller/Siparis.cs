using System.ComponentModel.DataAnnotations;

namespace ButikProjesi.API.Modeller
{
    /// <summary>
    /// Sipariş bilgilerini tutan model sınıfı
    /// </summary>
    public class Siparis
    {
        /// <summary>
        /// Sipariş benzersiz kimlik numarası
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Siparişi veren kullanıcının kimlik numarası
        /// </summary>
        [Required(ErrorMessage = "Kullanıcı kimlik numarası zorunludur")]
        public string KullaniciId { get; set; } = string.Empty;

        /// <summary>
        /// Sipariş tarihi
        /// </summary>
        [Required(ErrorMessage = "Sipariş tarihi zorunludur")]
        public DateTime SiparisTarihi { get; set; } = DateTime.Now;

        /// <summary>
        /// Sipariş toplam tutarı
        /// </summary>
        [Required(ErrorMessage = "Toplam tutar zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Toplam tutar 0'dan büyük olmalıdır")]
        public decimal ToplamTutar { get; set; }

        /// <summary>
        /// Sipariş durumu (Hazırlanıyor, Kargolandı, Teslim Edildi vb.)
        /// </summary>
        [Required(ErrorMessage = "Sipariş durumu zorunludur")]
        [StringLength(50, ErrorMessage = "Sipariş durumu en fazla 50 karakter olabilir")]
        public string Durum { get; set; } = "Hazırlanıyor";

        /// <summary>
        /// Teslimat için ad soyad
        /// </summary>
        [Required(ErrorMessage = "Ad soyad zorunludur")]
        [StringLength(100, ErrorMessage = "Ad soyad en fazla 100 karakter olabilir")]
        public string AdSoyad { get; set; } = string.Empty;

        /// <summary>
        /// Teslimat için telefon
        /// </summary>
        [Required(ErrorMessage = "Telefon zorunludur")]
        [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir")]
        public string Telefon { get; set; } = string.Empty;

        /// <summary>
        /// Teslimat için e-posta
        /// </summary>
        [Required(ErrorMessage = "E-posta zorunludur")]
        [StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir")]
        public string Eposta { get; set; } = string.Empty;

        /// <summary>
        /// Teslimat adresi
        /// </summary>
        [Required(ErrorMessage = "Adres zorunludur")]
        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir")]
        public string Adres { get; set; } = string.Empty;

        /// <summary>
        /// Teslimat şehri
        /// </summary>
        [Required(ErrorMessage = "Şehir zorunludur")]
        [StringLength(50, ErrorMessage = "Şehir en fazla 50 karakter olabilir")]
        public string Sehir { get; set; } = string.Empty;

        /// <summary>
        /// Posta kodu
        /// </summary>
        [Required(ErrorMessage = "Posta kodu zorunludur")]
        [StringLength(10, ErrorMessage = "Posta kodu en fazla 10 karakter olabilir")]
        public string PostaKodu { get; set; } = string.Empty;

        /// <summary>
        /// Sipariş notu (opsiyonel)
        /// </summary>
        [StringLength(500, ErrorMessage = "Sipariş notu en fazla 500 karakter olabilir")]
        public string? SiparisNotu { get; set; }

        /// <summary>
        /// Siparişin teslim edildiği tarih (Sadece "Teslim Edildi" durumunda dolu)
        /// </summary>
        public DateTime? TeslimTarihi { get; set; }

        /// <summary>
        /// Bu siparişe ait sipariş kalemleri (Navigation Property)
        /// </summary>
        public virtual ICollection<SiparisKalemi> SiparisKalemleri { get; set; } = new List<SiparisKalemi>();
    }
}
