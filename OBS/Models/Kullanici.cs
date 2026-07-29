using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class Kullanici
{
    public int Id { get; set; }

    public string Ad { get; set; } = null!;

    public string Soyad { get; set; } = null!;

    public string Eposta { get; set; } = null!;

    public string Telefon { get; set; } = null!;

    public string? SifreHash { get; set; }

    public bool IkiFaktorluDogrulama { get; set; }

    public bool AktiflikDurumu { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public DateTime? SonGuncellenmeTarihi { get; set; }

    public virtual ICollection<DenetimKaydi> DenetimKaydis { get; set; } = new List<DenetimKaydi>();

    public virtual ICollection<KullaniciRol> KullaniciRols { get; set; } = new List<KullaniciRol>();

    public virtual ICollection<KullaniciYetki> KullaniciYetkiIslemYapanKullanicis { get; set; } = new List<KullaniciYetki>();

    public virtual ICollection<KullaniciYetki> KullaniciYetkiKullanicis { get; set; } = new List<KullaniciYetki>();

    public virtual Ogrenci? Ogrenci { get; set; }

    public virtual OgretimUyesi? OgretimUyesi { get; set; }
}
