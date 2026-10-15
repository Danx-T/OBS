using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class AcilanDer
{
    public int Id { get; set; }

    public int DersId { get; set; }

    public int OgretimUyesiId { get; set; }

    public int DonemId { get; set; }

    public string SubeNo { get; set; } = null!;

    public int Kontenjan { get; set; }

    public string? Durum { get; set; }

    public virtual Der Ders { get; set; } = null!;

    public virtual ICollection<DersKaydi> DersKaydis { get; set; } = new List<DersKaydi>();

    public virtual ICollection<DersProgrami> DersProgramis { get; set; } = new List<DersProgrami>();

    public virtual Donem Donem { get; set; } = null!;

    public virtual OgretimUyesi OgretimUyesi { get; set; } = null!;

    public virtual ICollection<SinavProgrami> SinavProgramis { get; set; } = new List<SinavProgrami>();
}
