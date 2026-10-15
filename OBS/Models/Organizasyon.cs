using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class Organizasyon
{
    public int Id { get; set; }

    public int? UstOrganizasyonId { get; set; }

    public string Tipi { get; set; } = null!;

    public string Adi { get; set; } = null!;

    public string? Kodu { get; set; }

    public bool Durum { get; set; }

    public virtual ICollection<Ders> Ders { get; set; } = new List<Ders>();

    public virtual ICollection<Organizasyon> InverseUstOrganizasyon { get; set; } = new List<Organizasyon>();

    public virtual ICollection<Ogrenci> Ogrencis { get; set; } = new List<Ogrenci>();

    public virtual ICollection<OgretimUyesi> OgretimUyesis { get; set; } = new List<OgretimUyesi>();

    public virtual Organizasyon? UstOrganizasyon { get; set; }
}
