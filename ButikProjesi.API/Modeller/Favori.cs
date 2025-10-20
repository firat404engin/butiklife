using System.ComponentModel.DataAnnotations;

namespace ButikProjesi.API.Modeller
{
    /// <summary>
    /// Kullanıcıların favori ürünlerini tutan model sınıfı
    /// </summary>
    public class Favori
    {
        /// <summary>
        /// Favori kaydının benzersiz kimlik numarası
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Favoriye ekleyen kullanıcının kimlik numarası
        /// </summary>
        [Required(ErrorMessage = "Kullanıcı ID'si zorunludur")]
        public string KullaniciId { get; set; } = string.Empty;

        /// <summary>
        /// Favoriye eklenen ürünün kimlik numarası
        /// </summary>
        [Required(ErrorMessage = "Ürün ID'si zorunludur")]
        public int UrunId { get; set; }

        /// <summary>
        /// Favoriye eklenme tarihi
        /// </summary>
        public DateTime EklenmeTarihi { get; set; } = DateTime.Now;

        /// <summary>
        /// Favoriye eklendiği andaki ürün fiyatı (fiyat takibi için)
        /// </summary>
        public decimal FiyatEklendiginde { get; set; }

        /// <summary>
        /// İlgili ürün (Navigation Property)
        /// </summary>
        public virtual Urun? Urun { get; set; }
    }
}









