using OBS.Models;

namespace OBS.ViewModels.Student
{
    public class StudentDashboardViewModel
    {
        public Ogrenci Ogrenci { get; set; } = null!;
        public Donem? AktifDonem { get; set; }
        public OgretimUyesi? Danisman { get; set; }
        public List<DersProgrami> BugunkuDersler { get; set; } = new List<DersProgrami>();
    }
}
