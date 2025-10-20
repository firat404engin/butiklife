namespace ButikProjesi.API.Modeller
{
    /// <summary>
    /// Ürün bilgilerini ve yorumlardan hesaplanan istatistikleri içeren DTO sınıfı
    /// </summary>
    public class UrunDto
    {
        /// <summary>
        /// Ürün benzersiz kimlik numarası
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Ürün adı
        /// </summary>
        public string Ad { get; set; } = string.Empty;

        /// <summary>
        /// Ürün açıklaması
        /// </summary>
        public string Aciklama { get; set; } = string.Empty;

        /// <summary>
        /// Ürün fiyatı
        /// </summary>
        public decimal Fiyat { get; set; }

        /// <summary>
        /// Ürünün indirimsiz (eski) fiyatı. Boş olabilir.
        /// </summary>
        public decimal? EskiFiyat { get; set; }

        /// <summary>
        /// Stok adedi
        /// </summary>
        public int StokAdedi { get; set; }

        /// <summary>
        /// Ürün görseli URL
        /// </summary>
        public string GorselUrl { get; set; } = string.Empty;

        /// <summary>
        /// Kategori ID
        /// </summary>
        public int KategoriId { get; set; }

        /// <summary>
        /// Kategori bilgisi
        /// </summary>
        public Kategori? Kategori { get; set; }

        /// <summary>
        /// Ürünün ortalama puanı (yorumlardan hesaplanır)
        /// </summary>
        public double OrtalamaPuan { get; set; }

        /// <summary>
        /// Ürün için yapılmış toplam yorum sayısı
        /// </summary>
        public int YorumSayisi { get; set; }
    }
}



