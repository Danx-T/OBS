using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class OgretimUyesi
{
    public int Id { get; set; }

    public int KullaniciId { get; set; }

    public string Cinsiyet { get; set; } = null!;

    public string? Unvan { get; set; }

    public int OrganizasyonId { get; set; }

    public string? KadroTipi { get; set; }

    public DateOnly GorevBaslangic { get; set; }

    public DateOnly? GorevBitis { get; set; }

    public virtual ICollection<AcilanDer> AcilanDers { get; set; } = new List<AcilanDer>();

    public virtual Kullanici Kullanici { get; set; } = null!;

    public virtual ICollection<Ogrenci> Ogrencis { get; set; } = new List<Ogrenci>();

    public virtual Organizasyon Organizasyon { get; set; } = null!;
}
