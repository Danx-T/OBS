using System.ComponentModel.DataAnnotations;
using OBS.Models;

namespace OBS.ViewModels;

public class RolYetkiYonetimiViewModel
{
    public List<Rol> Roller { get; set; } = new();
    public List<Yetki> Yetkiler { get; set; } = new();
    public List<RolYetki> RolYetkiler { get; set; } = new();
}

public class RolEkleModel
{
    [Required(ErrorMessage = "Rol adı zorunludur.")]
    [StringLength(50, ErrorMessage = "Rol adı en fazla 50 karakter olabilir.")]
    [Display(Name = "Rol Adı")]
    public string RolAdi { get; set; } = null!;

    [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }
}

public class YetkiEkleModel
{
    [Required(ErrorMessage = "Yetki kodu zorunludur.")]
    [StringLength(50, ErrorMessage = "Yetki kodu en fazla 50 karakter olabilir.")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Yetki kodu sadece büyük harf, rakam ve alt çizgi içerebilir (Örn: NOT_GIRIS).")]
    [Display(Name = "Yetki Kodu")]
    public string YetkiKodu { get; set; } = null!;

    [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }
}

public class RolYetkiAtamaModel
{
    public int RolId { get; set; }
    public List<int> SeciliYetkiIdler { get; set; } = new();
}
