namespace ButikProjesi.Shared.Modeller
{
    public class MusteriDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AdSoyad { get; set; } = string.Empty;
        public int SiparisAdeti { get; set; }
        public DateTime KayitTarihi { get; set; }
    }
}
