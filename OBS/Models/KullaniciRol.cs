using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class KullaniciRol
{
    public int KullaniciId { get; set; }

    public int RolId { get; set; }

    public bool AktiflikDurumu { get; set; }

    public DateTime BaslangicTarihi { get; set; }

    public DateTime? BitisTarihi { get; set; }

    public virtual Kullanici Kullanici { get; set; } = null!;

    public virtual Rol Rol { get; set; } = null!;
}
