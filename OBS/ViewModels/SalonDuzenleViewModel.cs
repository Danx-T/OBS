using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels
{
    public class SalonDuzenleViewModel
    {
        public int Id { get; set; }

        public int? BinaId { get; set; }

        [Required(ErrorMessage = "Salon Tipi zorunludur.")]
        [StringLength(20)]
        public string SalonTipi { get; set; } = null!;

        [Required(ErrorMessage = "Adı zorunludur.")]
        [StringLength(20)]
        public string SalonAdi { get; set; } = null!;

        public short? Kapasite { get; set; }
    }
}
