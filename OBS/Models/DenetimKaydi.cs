using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class DenetimKaydi
{
    public int Id { get; set; }

    public int? KullaniciId { get; set; }

    public string IslemTuru { get; set; } = null!;

    public string EtkilenenTablo { get; set; } = null!;

    public int EtkilenenKayitId { get; set; }

    public string EtkilenenSutun { get; set; } = null!;

    public string? EskiDeger { get; set; }

    public string? YeniDeger { get; set; }

    public DateTime IslemZamani { get; set; }

    public string? IpAdresi { get; set; }

    public virtual Kullanici? Kullanici { get; set; }
}
