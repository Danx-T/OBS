using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class Ogrenci
{
    public int Id { get; set; }

    public int KullaniciId { get; set; }

    public string Cinsiyet { get; set; } = null!;

    public string OgrenciNo { get; set; } = null!;

    public int? DanismanId { get; set; }

    public int OrganizasyonId { get; set; }

    public DateOnly GirisTarihi { get; set; }

    public string OgrenciTipi { get; set; } = null!;

    public string Durum { get; set; } = null!;

    public DateOnly? MezuniyetTarihi { get; set; }

    public int Sinif { get; set; }

    public virtual OgretimUyesi? Danisman { get; set; }

    public virtual ICollection<DersKaydi> DersKaydis { get; set; } = new List<DersKaydi>();

    public virtual Kullanici Kullanici { get; set; } = null!;

    public virtual Organizasyon Organizasyon { get; set; } = null!;
}
