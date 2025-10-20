namespace ButikProjesi.Istemci.Modeller
{
    /// <summary>
    /// Sepetteki bir ürünü temsil eden sınıf
    /// </summary>
    public class SepetUrunu
    {
        /// <summary>
        /// Ürün ID'si
        /// </summary>
        public int UrunId { get; set; }

        /// <summary>
        /// Ürün adı
        /// </summary>
        public string Ad { get; set; } = string.Empty;

        /// <summary>
        /// Ürün fiyatı
        /// </summary>
        public decimal Fiyat { get; set; }

        /// <summary>
        /// Ürün görsel URL'si
        /// </summary>
        public string GorselUrl { get; set; } = string.Empty;

        /// <summary>
        /// Sepetteki adet
        /// </summary>
        public int Adet { get; set; }

        /// <summary>
        /// Toplam fiyat (Fiyat * Adet)
        /// </summary>
        public decimal ToplamFiyat => Fiyat * Adet;
    }
}










