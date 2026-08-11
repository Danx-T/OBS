using OBS.Models;

namespace OBS.ViewModels.Academician
{
    public class AdvisorStudentRegistrationDto
    {
        public Ogrenci Ogrenci { get; set; } = null!;
        public List<DersKaydi> DersKayitlari { get; set; } = new List<DersKaydi>();
        public string GenelDurum { get; set; } = "Onay Bekliyor"; 
    }

    public class AcademicianAdvisorViewModel
    {
        public OgretimUyesi OgretimUyesi { get; set; } = null!;
        public Donem? AktifDonem { get; set; }
        
        public List<AdvisorStudentRegistrationDto> OnayBekleyenler { get; set; } = new();
        public List<AdvisorStudentRegistrationDto> Onaylananlar { get; set; } = new();
        public List<AdvisorStudentRegistrationDto> Reddedilenler { get; set; } = new();
    }
}
