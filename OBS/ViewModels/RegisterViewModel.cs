using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad zorunludur.")]
    [MaxLength(100)]
    public string Ad { get; set; } = null!;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    [MaxLength(100)]
    public string Soyad { get; set; } = null!;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [MaxLength(50)]
    public string Eposta { get; set; } = null!;

    [Required(ErrorMessage = "Telefon zorunludur.")]
    [RegularExpression(@"^(\+90|0)[0-9]{10}$", ErrorMessage = "Geçerli bir telefon numarası giriniz. (Örn: 05321234567)")]
    [MaxLength(20)]
    public string Telefon { get; set; } = null!;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
        ErrorMessage = "Şifre en az 8 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.")]
    [DataType(DataType.Password)]
    public string Sifre { get; set; } = null!;

    [Required(ErrorMessage = "Şifre onayı zorunludur.")]
    [DataType(DataType.Password)]
    [Compare("Sifre", ErrorMessage = "Şifreler eşleşmiyor.")]
    public string SifreOnay { get; set; } = null!;
}
