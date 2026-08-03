using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels;

public class KullaniciOlusturViewModel
{
    // ── Kullanıcı Temel Bilgileri ───────────────────────────
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

    /// <summary>Hiçbiri (null) | Ogrenci | OgretimUyesi</summary>
    public string? KullaniciTipi { get; set; }

    // ── Ortak Alanlar (Öğrenci veya Öğretim Üyesi ise geçerli) ──
    /// <summary>Kadin | Erkek</summary>
    public string? Cinsiyet { get; set; }

    public int? OrganizasyonId { get; set; }

    // ── Öğrenci Alanları ────────────────────────────────────
    public string? OgrenciNo { get; set; }

    public int? DanismanId { get; set; }

    public DateOnly? GirisTarihi { get; set; }

    /// <summary>Normal | Yatay Gecis | Uluslararasi</summary>
    public string? OgrenciTipi { get; set; }

    /// <summary>Aktif | Mezun | Kayit Dondurmus | Ilisigi Kesilmis</summary>
    public string? OgrenciDurum { get; set; }

    public int? Sinif { get; set; }

    // ── Öğretim Üyesi Alanları ──────────────────────────────
    /// <summary>Ogr. Gor. | Dr. Ogr. Uyesi | Doc. Dr. | Prof. Dr.</summary>
    public string? Unvan { get; set; }

    /// <summary>Kadrolu | Sozlesmeli | Yari Zamanli | Misafir Ogretim Uyesi</summary>
    public string? KadroTipi { get; set; }

    public DateOnly? GorevBaslangic { get; set; }

    public DateOnly? GorevBitis { get; set; }
}
