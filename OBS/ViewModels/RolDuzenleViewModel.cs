using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels
{
    public class RolDuzenleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Rol Adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Rol adı en fazla 50 karakter olabilir.")]
        public string RolAdi { get; set; } = null!;

        [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
        public string? Aciklama { get; set; }
    }
}
