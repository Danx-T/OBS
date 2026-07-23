using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class DersKaydi
{
    public int Id { get; set; }

    public int OgrenciId { get; set; }

    public int AcilanDersId { get; set; }

    public string KayitDurumu { get; set; } = null!;

    public DateTime KayitTarihi { get; set; }

    public DateTime? OnayTarihi { get; set; }

    public virtual AcilanDer AcilanDers { get; set; } = null!;

    public virtual ICollection<Notlar> Notlars { get; set; } = new List<Notlar>();

    public virtual Ogrenci Ogrenci { get; set; } = null!;
}
