namespace ButikProjesi.Shared.Modeller
{
    public class Urun
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public decimal Fiyat { get; set; }
        public decimal? EskiFiyat { get; set; }
        public int StokAdedi { get; set; }
        public string? GorselUrl { get; set; }
        public int KategoriId { get; set; }
        public Kategori? Kategori { get; set; }
    }
}










