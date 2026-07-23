using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class Salon
{
    public int Id { get; set; }

    public int? BinaId { get; set; }

    public string SalonTipi { get; set; } = null!;

    public string SalonAdi { get; set; } = null!;

    public short? Kapasite { get; set; }

    public virtual Salon? Bina { get; set; }

    public virtual ICollection<DersProgrami> DersProgramis { get; set; } = new List<DersProgrami>();

    public virtual ICollection<Salon> InverseBina { get; set; } = new List<Salon>();

    public virtual ICollection<SinavProgrami> SinavProgramis { get; set; } = new List<SinavProgrami>();
}
