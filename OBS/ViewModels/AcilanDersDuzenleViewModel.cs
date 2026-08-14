using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels
{
    public class AcilanDersDuzenleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ders seçimi zorunludur.")]
        public int DersId { get; set; }

        [Required(ErrorMessage = "Öğretim Üyesi seçimi zorunludur.")]
        public int OgretimUyesiId { get; set; }

        [Required(ErrorMessage = "Dönem seçimi zorunludur.")]
        public int DonemId { get; set; }

        [Required(ErrorMessage = "Şube No zorunludur.")]
        [StringLength(5, ErrorMessage = "Şube No en fazla 5 karakter olabilir.")]
        public string SubeNo { get; set; } = null!;

        [Required(ErrorMessage = "Kontenjan zorunludur.")]
        [Range(1, 1000, ErrorMessage = "Kontenjan en az 1 olmalıdır.")]
        public int Kontenjan { get; set; }

        public string? Durum { get; set; }
    }
}
