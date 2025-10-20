using System.ComponentModel.DataAnnotations;

namespace ButikProjesi.API.Modeller
{
    /// <summary>
    /// Kategori bilgilerini tutan model sınıfı
    /// </summary>
    public class Kategori
    {
        /// <summary>
        /// Kategori benzersiz kimlik numarası
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Kategori adı
        /// </summary>
        [Required(ErrorMessage = "Kategori adı zorunludur")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir")]
        public string Ad { get; set; } = string.Empty;

        /// <summary>
        /// Kategori görsel URL'si
        /// </summary>
        public string GorselUrl { get; set; } = string.Empty;

        /// <summary>
        /// Bu kategoriye ait ürünler (Navigation Property)
        /// </summary>
        public virtual ICollection<Urun> Urunler { get; set; } = new List<Urun>();
    }
}
