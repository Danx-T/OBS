using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels;

public class SetPasswordViewModel
{
    [Required]
    public string Token { get; set; } = null!;

    public string? AdSoyad { get; set; }
    public string? Eposta { get; set; }

    [Required(ErrorMessage = "Şifre alanı zorunludur.")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
        ErrorMessage = "Şifre en az 8 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.")]
    [Display(Name = "Yeni Şifre")]
    public string Sifre { get; set; } = null!;

    [Required(ErrorMessage = "Şifre tekrar alanı zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre (Tekrar)")]
    [Compare(nameof(Sifre), ErrorMessage = "Şifreler birbiriyle eşleşmiyor.")]
    public string SifreTekrar { get; set; } = null!;
}
