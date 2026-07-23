using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class KullaniciYetki
{
    public int KullaniciId { get; set; }

    public int YetkiId { get; set; }

    public int? IslemYapanKullaniciId { get; set; }

    public DateTime BaslangicTarihi { get; set; }

    public DateTime? BitisTarihi { get; set; }

    public virtual Kullanici? IslemYapanKullanici { get; set; }

    public virtual Kullanici Kullanici { get; set; } = null!;

    public virtual Yetki Yetki { get; set; } = null!;
}
