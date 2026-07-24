using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels;

public class VerifyViewModel
{
    /// <summary>Hangi kullanıcıya ait doğrulama ekranı olduğunu tutan gizli alan.</summary>
    public int KullaniciId { get; set; }

    [Required(ErrorMessage = "Lütfen doğrulama kodunu giriniz.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Kod 6 haneli olmalıdır.")]
    [Display(Name = "Doğrulama Kodu")]
    public string Kod { get; set; } = null!;
}
