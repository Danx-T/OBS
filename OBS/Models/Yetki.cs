using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class Yetki
{
    public int Id { get; set; }

    public string YetkiKodu { get; set; } = null!;

    public string? Aciklama { get; set; }

    public virtual ICollection<KullaniciYetki> KullaniciYetkis { get; set; } = new List<KullaniciYetki>();

    public virtual ICollection<RolYetki> RolYetkis { get; set; } = new List<RolYetki>();
}
