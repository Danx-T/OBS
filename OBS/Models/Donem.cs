using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class Donem
{
    public int Id { get; set; }

    public string AkademikYil { get; set; } = null!;

    public string Donem1 { get; set; } = null!;

    public DateOnly BaslangicTarihi { get; set; }

    public DateOnly BitisTarihi { get; set; }

    public DateTime DersKaydiBaslangic { get; set; }

    public DateTime DersKaydiBitis { get; set; }

    public virtual ICollection<AcilanDer> AcilanDers { get; set; } = new List<AcilanDer>();
}
