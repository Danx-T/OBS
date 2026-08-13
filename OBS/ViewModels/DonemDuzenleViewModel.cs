using System;
using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels
{
    public class DonemDuzenleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Akademik Yıl zorunludur.")]
        [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "Örn: 2025-2026 formatında olmalıdır.")]
        [StringLength(9)]
        public string AkademikYil { get; set; } = null!;

        [Required(ErrorMessage = "Dönem zorunludur.")]
        [StringLength(10)]
        public string Donem1 { get; set; } = null!;

        [Required(ErrorMessage = "Başlangıç Tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateOnly BaslangicTarihi { get; set; }

        [Required(ErrorMessage = "Bitiş Tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateOnly BitisTarihi { get; set; }

        [Required(ErrorMessage = "Ders Kaydı Başlangıç Tarihi zorunludur.")]
        public DateTime DersKaydiBaslangic { get; set; }

        [Required(ErrorMessage = "Ders Kaydı Bitiş Tarihi zorunludur.")]
        public DateTime DersKaydiBitis { get; set; }
    }
}
