using OBS.Models;

namespace OBS.ViewModels.Student
{
    public class CourseRegistrationViewModel
    {
        public Ogrenci Ogrenci { get; set; } = null!;
        public Donem? AktifDonem { get; set; }
        
        // Bu dönem açılan dersler
        public List<AcilanDer> AcilanDersler { get; set; } = new List<AcilanDer>();
        
        // Cache'te tutulan, öğrencinin eklediği dersler
        public List<AcilanDer> SecilenDersler { get; set; } = new List<AcilanDer>();
        
        // Veritabanında kesinleşmiş (Onay Bekliyor, Onaylandı, vs.) kayıtlar
        public List<DersKaydi> MevcutKayitlar { get; set; } = new List<DersKaydi>();
        
        public string? HataMesaji { get; set; }
        public string? BasariMesaji { get; set; }
        public bool IsChecked { get; set; }
    }
}
