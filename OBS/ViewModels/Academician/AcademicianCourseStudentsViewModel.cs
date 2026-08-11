using OBS.Models;

namespace OBS.ViewModels.Academician
{
    public class AcademicianCourseStudentsViewModel
    {
        public OgretimUyesi OgretimUyesi { get; set; } = null!;
        public AcilanDer AcilanDers { get; set; } = null!;
        public List<DersKaydi> DersKayitlari { get; set; } = new List<DersKaydi>();
    }
}
