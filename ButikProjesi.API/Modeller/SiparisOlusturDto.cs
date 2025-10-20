namespace ButikProjesi.API.Modeller
{
    /// <summary>
    /// Sipariş oluşturma için kullanılan veri transfer nesnesi
    /// </summary>
    public class SiparisOlusturDto
    {
        /// <summary>
        /// Müşterinin ad soyad bilgisi
        /// </summary>
        public string AdSoyad { get; set; } = string.Empty;

        /// <summary>
        /// Müşterinin telefon numarası
        /// </summary>
        public string Telefon { get; set; } = string.Empty;

        /// <summary>
        /// Müşterinin e-posta adresi
        /// </summary>
        public string Eposta { get; set; } = string.Empty;

        /// <summary>
        /// Teslimat adresi
        /// </summary>
        public string Adres { get; set; } = string.Empty;

        /// <summary>
        /// Teslimat şehri
        /// </summary>
        public string Sehir { get; set; } = string.Empty;

        /// <summary>
        /// Posta kodu
        /// </summary>
        public string PostaKodu { get; set; } = string.Empty;

        /// <summary>
        /// Sipariş notu (opsiyonel)
        /// </summary>
        public string? SiparisNotu { get; set; }

        /// <summary>
        /// Sepetteki ürünler
        /// </summary>
        public List<SepetUrunDto> SepetUrunleri { get; set; } = new();
    }

    /// <summary>
    /// Sepetteki bir ürünü temsil eden DTO
    /// </summary>
    public class SepetUrunDto
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
        /// Sipariş edilen adet
        /// </summary>
        public int Adet { get; set; }
    }
}











