using System;
using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels
{
    public class SinavProgramiDuzenleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ders seçimi zorunludur.")]
        public int AcilanDersId { get; set; }

        [Required(ErrorMessage = "Salon seçimi zorunludur.")]
        public int SalonId { get; set; }

        [Required(ErrorMessage = "Sınav tipi zorunludur.")]
        [StringLength(15)]
        public string SinavTipi { get; set; } = null!;

        [Required(ErrorMessage = "Başlangıç tarihi ve saati zorunludur.")]
        public DateTime Baslangic { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi ve saati zorunludur.")]
        public DateTime Bitis { get; set; }
    }
}
