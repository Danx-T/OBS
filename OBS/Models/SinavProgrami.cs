using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class SinavProgrami
{
    public int Id { get; set; }

    public int AcilanDersId { get; set; }

    public int SalonId { get; set; }

    public string SinavTipi { get; set; } = null!;

    public DateTime Baslangic { get; set; }

    public DateTime Bitis { get; set; }

    public virtual AcilanDer AcilanDers { get; set; } = null!;

    public virtual Salon Salon { get; set; } = null!;
}
