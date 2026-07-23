using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class DersProgrami
{
    public int Id { get; set; }

    public int AcilanDersId { get; set; }

    public int SalonId { get; set; }

    public string Gun { get; set; } = null!;

    public TimeOnly BaslangicSaati { get; set; }

    public TimeOnly BitisSaati { get; set; }

    public virtual AcilanDer AcilanDers { get; set; } = null!;

    public virtual Salon Salon { get; set; } = null!;
}
