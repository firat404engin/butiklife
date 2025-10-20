using System.ComponentModel.DataAnnotations;

namespace ButikProjesi.API.Modeller
{
    /// <summary>
    /// Sipariş durumu güncelleme için DTO
    /// </summary>
    public class SiparisDurumDto
    {
        /// <summary>
        /// Siparişin yeni durumu
        /// </summary>
        [Required(ErrorMessage = "Yeni durum bilgisi zorunludur")]
        public string YeniDurum { get; set; } = string.Empty;
    }
}











