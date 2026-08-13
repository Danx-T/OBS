using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels
{
    public class OrganizasyonDuzenleViewModel
    {
        public int Id { get; set; }

        public int? UstOrganizasyonId { get; set; }

        [Required(ErrorMessage = "Organizasyon Tipi zorunludur.")]
        [StringLength(10)]
        public string Tipi { get; set; } = null!;

        [Required(ErrorMessage = "Organizasyon Adı zorunludur.")]
        [StringLength(100)]
        public string Adi { get; set; } = null!;

        [StringLength(10)]
        public string? Kodu { get; set; }

        public bool Durum { get; set; }
    }
}
