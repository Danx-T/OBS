using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class RolYetki
{
    public int RolId { get; set; }

    public int YetkiId { get; set; }

    public DateTime BaslangicTarihi { get; set; }

    public DateTime? BitisTarihi { get; set; }

    public virtual Rol Rol { get; set; } = null!;

    public virtual Yetki Yetki { get; set; } = null!;
}
