using System;
using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels
{
    public class OgrenciDuzenleViewModel
    {
        public int Id { get; set; } // KullaniciId
        public int OgrenciId { get; set; } // Ogrenci tablosu PK

        public string Ad { get; set; } = null!; // Gösterim amaçlı
        public string Soyad { get; set; } = null!; // Gösterim amaçlı

        [Required(ErrorMessage = "Cinsiyet seçimi zorunludur.")]
        public string Cinsiyet { get; set; } = null!;

        [Required(ErrorMessage = "Öğrenci Numarası zorunludur.")]
        [StringLength(15)]
        public string OgrenciNo { get; set; } = null!;

        public int? DanismanId { get; set; }

        [Required(ErrorMessage = "Bölüm/Organizasyon seçimi zorunludur.")]
        public int OrganizasyonId { get; set; }

        [Required(ErrorMessage = "Giriş Tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateOnly GirisTarihi { get; set; }

        [Required(ErrorMessage = "Öğrenci Tipi zorunludur.")]
        public string OgrenciTipi { get; set; } = null!;

        [Required(ErrorMessage = "Durum zorunludur.")]
        public string Durum { get; set; } = null!;

        [DataType(DataType.Date)]
        public DateOnly? MezuniyetTarihi { get; set; }

        [Required(ErrorMessage = "Sınıf bilgisi zorunludur.")]
        [Range(1, 10, ErrorMessage = "Sınıf 1 ile 10 arasında olmalıdır.")]
        public int Sinif { get; set; }
    }
}
