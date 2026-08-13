using System;
using System.ComponentModel.DataAnnotations;

namespace OBS.ViewModels
{
    public class OgretimUyesiDuzenleViewModel
    {
        public int Id { get; set; } // KullaniciId
        public int OgretimUyesiId { get; set; } // OgretimUyesi tablosu PK

        public string Ad { get; set; } = null!; // Gösterim amaçlı
        public string Soyad { get; set; } = null!; // Gösterim amaçlı

        [Required(ErrorMessage = "Cinsiyet seçimi zorunludur.")]
        public string Cinsiyet { get; set; } = null!;

        public string? Unvan { get; set; }

        [Required(ErrorMessage = "Bölüm/Organizasyon seçimi zorunludur.")]
        public int OrganizasyonId { get; set; }

        public string? KadroTipi { get; set; }

        [Required(ErrorMessage = "Görev Başlangıç Tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateOnly GorevBaslangic { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? GorevBitis { get; set; }
    }
}
