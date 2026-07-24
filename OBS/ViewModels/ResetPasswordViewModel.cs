using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels;

public class ResetPasswordViewModel
{
    public string Token { get; set; } = null!;

    [Required(ErrorMessage = "Yeni şifre zorunludur.")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
        ErrorMessage = "Şifre en az 8 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.")]
    [Display(Name = "Yeni Şifre")]
    public string YeniSifre { get; set; } = null!;

    [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
    [DataType(DataType.Password)]
    [Compare(nameof(YeniSifre), ErrorMessage = "Şifreler eşleşmiyor.")]
    [Display(Name = "Yeni Şifre (Tekrar)")]
    public string YeniSifreTekrar { get; set; } = null!;
}
