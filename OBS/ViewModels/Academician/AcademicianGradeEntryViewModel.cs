using OBS.Models;

namespace OBS.ViewModels.Academician
{
    public class AcademicianGradeEntryViewModel
    {
        public OgretimUyesi OgretimUyesi { get; set; } = null!;
        public Donem? AktifDonem { get; set; }
        public List<SinavProgrami> GirisYapilabilirSinavlar { get; set; } = new List<SinavProgrami>();
        public List<AcilanDers> VerilenDersler { get; set; } = new List<AcilanDers>();
    }

    public class AcademicianGradeEntryDetailViewModel
    {
        public OgretimUyesi OgretimUyesi { get; set; } = null!;
        public SinavProgrami? Sinav { get; set; }
        public AcilanDers? AcilanDers { get; set; }
        public string OlcmeTuru { get; set; } = "";
        public List<OgrenciNotDTO> OgrenciNotlari { get; set; } = new List<OgrenciNotDTO>();
    }

    public class OgrenciNotDTO
    {
        public DersKaydi DersKaydi { get; set; } = null!;
        public Notlar? MevcutNot { get; set; }
    }
}
