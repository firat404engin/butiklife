namespace ButikProjesi.Shared.Modeller
{
    /// <summary>
    /// Ürün bilgilerini ve yorumlardan hesaplanan istatistikleri içeren DTO sınıfı
    /// </summary>
    public class UrunDto
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
        public decimal Fiyat { get; set; }
        public decimal? EskiFiyat { get; set; }
        public int StokAdedi { get; set; }
        public string GorselUrl { get; set; } = string.Empty;
        public int KategoriId { get; set; }
        public Kategori? Kategori { get; set; }
        public double OrtalamaPuan { get; set; }
        public int YorumSayisi { get; set; }
        public int SatisSayisi { get; set; }
    }

    /// <summary>
    /// Basit kategori modeli (paylaşılan kullanım için)
    /// </summary>
    public class Kategori
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string GorselUrl { get; set; } = string.Empty;
    }
}


