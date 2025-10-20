using System.ComponentModel.DataAnnotations;

namespace ButikProjesi.API.Modeller
{
    /// <summary>
    /// Sipariş kalemi bilgilerini tutan model sınıfı
    /// </summary>
    public class SiparisKalemi
    {
        /// <summary>
        /// Sipariş kalemi benzersiz kimlik numarası
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Bu kalemin ait olduğu siparişin kimlik numarası
        /// </summary>
        [Required(ErrorMessage = "Sipariş kimlik numarası zorunludur")]
        public int SiparisId { get; set; }

        /// <summary>
        /// Bu kalemdeki ürünün kimlik numarası
        /// </summary>
        [Required(ErrorMessage = "Ürün kimlik numarası zorunludur")]
        public int UrunId { get; set; }

        /// <summary>
        /// Sipariş edilen ürün adedi
        /// </summary>
        [Required(ErrorMessage = "Ürün adedi zorunludur")]
        [Range(1, int.MaxValue, ErrorMessage = "Ürün adedi en az 1 olmalıdır")]
        public int Adet { get; set; }

        /// <summary>
        /// Bu kalemdeki ürünün birim fiyatı
        /// </summary>
        [Required(ErrorMessage = "Ürün fiyatı zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ürün fiyatı 0'dan büyük olmalıdır")]
        public decimal Fiyat { get; set; }

        /// <summary>
        /// Bu kalemin ait olduğu sipariş (Navigation Property)
        /// </summary>
        public virtual Siparis? Siparis { get; set; }

        /// <summary>
        /// Bu kalemdeki ürün (Navigation Property)
        /// </summary>
        public virtual Urun? Urun { get; set; }
    }
}
