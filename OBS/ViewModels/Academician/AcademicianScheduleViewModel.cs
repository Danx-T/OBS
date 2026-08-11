using OBS.Models;

namespace OBS.ViewModels.Academician
{
    public class AcademicianScheduleViewModel
    {
        public OgretimUyesi OgretimUyesi { get; set; } = null!;
        public Donem? AktifDonem { get; set; }
        public List<DersProgrami> HaftalikDersProgrami { get; set; } = new List<DersProgrami>();
    }
}
