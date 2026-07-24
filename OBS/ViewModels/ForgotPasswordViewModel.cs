using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string Eposta { get; set; } = null!;
}
