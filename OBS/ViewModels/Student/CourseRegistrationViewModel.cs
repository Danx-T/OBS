using OBS.Models;

namespace OBS.ViewModels.Student
{
    public class CourseRegistrationViewModel
    {
        public Ogrenci Ogrenci { get; set; } = null!;
        public Donem? AktifDonem { get; set; }
        
        // Bu dönem açılan dersler
        public List<AcilanDers> AcilanDersler { get; set; } = new List<AcilanDers>();
        
        // Cache'te tutulan, öğrencinin eklediği dersler
        public List<AcilanDers> SecilenDersler { get; set; } = new List<AcilanDers>();
        
        // Veritabanında kesinleşmiş (Onay Bekliyor, Onaylandı, vs.) kayıtlar
        public List<DersKaydi> MevcutKayitlar { get; set; } = new List<DersKaydi>();
        
        public string? HataMesaji { get; set; }
        public string? BasariMesaji { get; set; }
        public bool IsChecked { get; set; }
    }
}
