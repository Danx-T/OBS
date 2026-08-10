using OBS.Models;

namespace OBS.ViewModels.Student
{
    public class StudentGradesViewModel
    {
        public Ogrenci Ogrenci { get; set; } = null!;
        public Donem? AktifDonem { get; set; }
        public List<DersKaydi> DersKayitlari { get; set; } = new List<DersKaydi>();
    }
}
