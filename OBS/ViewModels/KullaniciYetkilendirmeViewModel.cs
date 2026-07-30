using System.ComponentModel.DataAnnotations;
using OBS.Models;

namespace OBS.ViewModels;

public class KullaniciYetkilendirmeViewModel
{
    public List<KullaniciYetkiOzetModel> Kullanicilar { get; set; } = new();
    public List<Rol> TumRoller { get; set; } = new();
    public List<Yetki> TumYetkiler { get; set; } = new();
    public string? Arama { get; set; }
    public int? BolumId { get; set; }
}

public class KullaniciYetkiOzetModel
{
    public int KullaniciId { get; set; }
    public string AdSoyad { get; set; } = null!;
    public string Eposta { get; set; } = null!;
    public string? BolumAdi { get; set; }
    public string? KullaniciTipi { get; set; }
    public List<string> Roller { get; set; } = new();
    public List<int> RolIdler { get; set; } = new();
    public List<string> DogrudanYetkiler { get; set; } = new();
    public List<int> DogrudanYetkiIdler { get; set; } = new();
}

public class KullaniciYetkilendirmeKaydetModel
{
    public int KullaniciId { get; set; }
    public List<int> SeciliRolIdler { get; set; } = new();
    public List<int> SeciliYetkiIdler { get; set; } = new();
    public string? Arama { get; set; }
    public int? BolumId { get; set; }
}
