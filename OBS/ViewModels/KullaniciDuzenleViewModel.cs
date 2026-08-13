using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels
{
    public class KullaniciDuzenleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [RegularExpression(@"^[a-pr-vy-zA-PR-VY-ZçÇğĞıİöÖşŞüÜ\s]+$", ErrorMessage = "Ad alanına rakam veya yabancı karakter yazılamaz. Sadece Türkçe harfler ve boşluk kullanılabilir.")]
        [MaxLength(100)]
        public string Ad { get; set; } = null!;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [RegularExpression(@"^[a-pr-vy-zA-PR-VY-ZçÇğĞıİöÖşŞüÜ\s]+$", ErrorMessage = "Soyad alanına rakam veya yabancı karakter yazılamaz. Sadece Türkçe harfler ve boşluk kullanılabilir.")]
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
}
