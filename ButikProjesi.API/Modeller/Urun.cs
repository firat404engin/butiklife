using System.ComponentModel.DataAnnotations;

namespace ButikProjesi.API.Modeller
{
    /// <summary>
    /// Ürün bilgilerini tutan model sınıfı
    /// </summary>
    public class Urun
    {
        /// <summary>
        /// Ürün benzersiz kimlik numarası
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Ürün adı
        /// </summary>
        [Required(ErrorMessage = "Ürün adı zorunludur")]
        [StringLength(200, ErrorMessage = "Ürün adı en fazla 200 karakter olabilir")]
        public string Ad { get; set; } = string.Empty;

        /// <summary>
        /// Ürün açıklaması
        /// </summary>
        [StringLength(1000, ErrorMessage = "Ürün açıklaması en fazla 1000 karakter olabilir")]
        public string? Aciklama { get; set; }

        /// <summary>
        /// Ürün fiyatı
        /// </summary>
        [Required(ErrorMessage = "Ürün fiyatı zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ürün fiyatı 0'dan büyük olmalıdır")]
        public decimal Fiyat { get; set; }

        /// <summary>
        /// Ürünün indirimsiz (eski) fiyatı. Boş olabilir.
        /// </summary>
        public decimal? EskiFiyat { get; set; }

        /// <summary>
        /// Stok adedi
        /// </summary>
        [Required(ErrorMessage = "Stok adedi zorunludur")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok adedi 0 veya pozitif olmalıdır")]
        public int StokAdedi { get; set; }

        /// <summary>
        /// Ürün görsel URL'si
        /// </summary>
        [StringLength(500, ErrorMessage = "Görsel URL'si en fazla 500 karakter olabilir")]
        public string? GorselUrl { get; set; }

        /// <summary>
        /// Ürünün ait olduğu kategori kimlik numarası
        /// </summary>
        [Required(ErrorMessage = "Kategori seçimi zorunludur")]
        public int KategoriId { get; set; }

        /// <summary>
        /// Ürünün ait olduğu kategori (Navigation Property)
        /// </summary>
        public virtual Kategori? Kategori { get; set; }

        /// <summary>
        /// Bu ürünün bulunduğu sipariş kalemleri (Navigation Property)
        /// </summary>
        public virtual ICollection<SiparisKalemi> SiparisKalemleri { get; set; } = new List<SiparisKalemi>();
    }
}
