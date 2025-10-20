using System.ComponentModel.DataAnnotations;

namespace ButikProjesi.Shared.Modeller
{
    public class Bildirim
    {
        public int Id { get; set; }
        
        [Required]
        public string KullaniciId { get; set; } = string.Empty;
        
        [Required]
        public int UrunId { get; set; }
        
        public virtual Urun Urun { get; set; } = null!; // Ürün bilgilerini çekmek için
        
        [Required]
        public string Mesaj { get; set; } = string.Empty;
        
        public bool OkunduMu { get; set; } = false;
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
    }
}


