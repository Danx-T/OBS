namespace OBS.ViewModels;

public class DanismanAtamaViewModel
{
    public int OgrenciId { get; set; }
    public string OgrenciNo { get; set; } = null!;
    public string AdSoyad { get; set; } = null!;
    public string BolumAdi { get; set; } = null!;
    public int Sinif { get; set; }
    public int? DanismanId { get; set; }
    public string? DanismanAdSoyad { get; set; }
}
