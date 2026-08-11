using OBS.Models;

namespace OBS.ViewModels.Academician
{
    public class AcademicianDashboardViewModel
    {
        public OgretimUyesi OgretimUyesi { get; set; } = null!;
        public Donem? AktifDonem { get; set; }
        public List<DersProgrami> BugunkuDersler { get; set; } = new List<DersProgrami>();
    }
}
