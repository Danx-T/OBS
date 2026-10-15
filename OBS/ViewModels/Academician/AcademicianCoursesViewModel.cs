using OBS.Models;

namespace OBS.ViewModels.Academician
{
    public class AcademicianCoursesViewModel
    {
        public OgretimUyesi OgretimUyesi { get; set; } = null!;
        public Donem? AktifDonem { get; set; }
        public List<AcilanDers> VerilenDersler { get; set; } = new List<AcilanDers>();
    }
}
