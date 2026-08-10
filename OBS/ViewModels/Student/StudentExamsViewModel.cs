using OBS.Models;

namespace OBS.ViewModels.Student
{
    public class StudentExamsViewModel
    {
        public Ogrenci Ogrenci { get; set; } = null!;
        public Donem? AktifDonem { get; set; }
        public List<SinavProgrami> Sinavlar { get; set; } = new List<SinavProgrami>();
    }
}
