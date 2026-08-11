using OBS.Models;

namespace OBS.ViewModels.Academician
{
    public class AcademicianExamsViewModel
    {
        public OgretimUyesi OgretimUyesi { get; set; } = null!;
        public Donem? AktifDonem { get; set; }
        public List<SinavProgrami> Sinavlar { get; set; } = new List<SinavProgrami>();
    }
}
