using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class Der
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

    public virtual ICollection<AcilanDer> AcilanDers { get; set; } = new List<AcilanDer>();

    public virtual Organizasyon Organizasyon { get; set; } = null!;

    public virtual ICollection<Der> Ders { get; set; } = new List<Der>();

    public virtual ICollection<Der> OnKosulDers { get; set; } = new List<Der>();
}
