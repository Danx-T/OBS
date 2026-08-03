namespace OBS.ViewModels;

public class KullaniciYonetimListeModel
{
    public int Id { get; set; }
    public string AdSoyad { get; set; } = null!;
    public string Eposta { get; set; } = null!;
    public string Telefon { get; set; } = null!;
    public bool AktiflikDurumu { get; set; }
    public string KullaniciTipi { get; set; } = null!;
    public string? BolumAdi { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public DateTime? SonGuncellenmeTarihi { get; set; }
    public bool IkiFaktorluDogrulama { get; set; }
    public List<string> Roller { get; set; } = new();
    public List<string> Yetkiler { get; set; } = new();
    public bool SifreBelirlenmisMi { get; set; }
}

public class KullaniciYonetimViewModel
{
    public List<KullaniciYonetimListeModel> Kullanicilar { get; set; } = new();
    public string? Arama { get; set; }
    public string? DurumFiltre { get; set; }  // "aktif", "pasif", "" (tümü)
    public string? TipFiltre { get; set; }    // "Ogrenci", "OgretimUyesi", "Diger", ""
}
