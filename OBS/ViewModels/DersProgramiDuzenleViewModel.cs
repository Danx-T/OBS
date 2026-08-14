using System;
using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels
{
    public class DersProgramiDuzenleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ders seçimi zorunludur.")]
        public int AcilanDersId { get; set; }

        [Required(ErrorMessage = "Salon seçimi zorunludur.")]
        public int SalonId { get; set; }

        [Required(ErrorMessage = "Gün seçimi zorunludur.")]
        public string Gun { get; set; } = null!;

        [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
        public TimeOnly BaslangicSaati { get; set; }

        [Required(ErrorMessage = "Bitiş saati zorunludur.")]
        public TimeOnly BitisSaati { get; set; }
    }
}
