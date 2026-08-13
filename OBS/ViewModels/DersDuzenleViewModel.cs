using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels
{
    public class DersDuzenleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Bağlı olduğu bölüm zorunludur.")]
        public int OrganizasyonId { get; set; }

        [Required(ErrorMessage = "Ders Kodu zorunludur.")]
        [StringLength(10)]
        public string DersKodu { get; set; } = null!;

        [Required(ErrorMessage = "Ders Adı zorunludur.")]
        [StringLength(100)]
        public string DersAdi { get; set; } = null!;

        [Required(ErrorMessage = "Kredi zorunludur.")]
        [Range(0, 10, ErrorMessage = "Kredi 0 ile 10 arasında olmalıdır.")]
        public decimal Kredi { get; set; }

        [Required(ErrorMessage = "AKTS zorunludur.")]
        [Range(0, 30, ErrorMessage = "AKTS 0 ile 30 arasında olmalıdır.")]
        public decimal Akts { get; set; }

        [Required(ErrorMessage = "Teorik saat zorunludur.")]
        public int Teorik { get; set; }

        [Required(ErrorMessage = "Uygulama saat zorunludur.")]
        public int Uygulama { get; set; }

        [Required(ErrorMessage = "Ders Türü zorunludur.")]
        [StringLength(10)]
        public string DersTuru { get; set; } = null!;

        public bool AktiflikDurumu { get; set; }
    }
}
