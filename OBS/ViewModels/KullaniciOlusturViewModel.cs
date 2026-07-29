using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels;

public class KullaniciOlusturViewModel
{
    [Required(ErrorMessage = "Ad zorunludur.")]
    [MaxLength(100)]
    public string Ad { get; set; } = null!;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    [MaxLength(100)]
    public string Soyad { get; set; } = null!;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    [MaxLength(50)]
    public string Eposta { get; set; } = null!;

    [Required(ErrorMessage = "Telefon zorunludur.")]
    [RegularExpression(@"^(\+90|0)[0-9]{10}$", ErrorMessage = "Geçerli bir telefon giriniz. (Örn: 05321234567)")]
    [MaxLength(20)]
    public string Telefon { get; set; } = null!;
}
