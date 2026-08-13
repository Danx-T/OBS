using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels
{
    public class YetkiDuzenleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Yetki Kodu zorunludur.")]
        [StringLength(50, ErrorMessage = "Yetki kodu en fazla 50 karakter olabilir.")]
        [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Yetki kodu sadece büyük harf, rakam ve alt çizgi içerebilir (Örn: NOT_GIRIS).")]
        public string YetkiKodu { get; set; } = null!;

        [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
        public string? Aciklama { get; set; }
    }
}
