using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class Rol
{
    public int Id { get; set; }

    public string RolAdi { get; set; } = null!;

    public string? Aciklama { get; set; }

    public virtual ICollection<KullaniciRol> KullaniciRols { get; set; } = new List<KullaniciRol>();

    public virtual ICollection<RolYetki> RolYetkis { get; set; } = new List<RolYetki>();
}
