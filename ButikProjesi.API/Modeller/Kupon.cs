using System.ComponentModel.DataAnnotations;

namespace ButikProjesi.API.Modeller
{
    public enum IndirimTipi 
    { 
        Yuzde, 
        SabitTutar 
    }

    public class Kupon
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Kod { get; set; } = string.Empty;
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Deger { get; set; }
        
        [Required]
        public IndirimTipi Tipi { get; set; }
        
        public bool AktifMi { get; set; } = true;
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? SonGecerlilikTarihi { get; set; }
    }
}