using OBS.Models;

namespace OBS.ViewModels.Student
{
    public class StudentScheduleViewModel
    {
        public Ogrenci Ogrenci { get; set; } = null!;
        public Donem? AktifDonem { get; set; }
        public List<DersProgrami> HaftalikDersProgrami { get; set; } = new List<DersProgrami>();
    }
}
