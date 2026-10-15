using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class Ders
{
    public int Id { get; set; }

    public int OrganizasyonId { get; set; }

    public string DersKodu { get; set; } = null!;

    public string DersAdi { get; set; } = null!;

    public decimal Kredi { get; set; }

    public decimal Akts { get; set; }

    public int Teorik { get; set; }

    public int Uygulama { get; set; }

    public string DersTuru { get; set; } = null!;

    public bool AktiflikDurumu { get; set; }

    public virtual ICollection<AcilanDers> AcilanDers { get; set; } = new List<AcilanDers>();

    public virtual Organizasyon Organizasyon { get; set; } = null!;

    public virtual ICollection<Ders> OnKosulOlduguDersler { get; set; } = new List<Ders>();

    public virtual ICollection<Ders> OnKosulDers { get; set; } = new List<Ders>();
}
