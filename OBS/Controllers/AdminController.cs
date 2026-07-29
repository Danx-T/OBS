using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;
using OBS.ViewModels;

namespace OBS.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ObsContext _context;

    public AdminController(ObsContext context)
    {
        _context = context;
    }

    // GET: /Admin/KullaniciOlustur
    [HttpGet]
    public IActionResult KullaniciOlustur()
    {
        return View(new KullaniciOlusturViewModel());
    }

    // POST: /Admin/KullaniciOlustur
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KullaniciOlustur(KullaniciOlusturViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // E-posta benzersizlik kontrolü
        if (await _context.Kullanicis.AnyAsync(k => k.Eposta == model.Eposta))
        {
            ModelState.AddModelError("Eposta", "Bu e-posta adresi zaten kayıtlı.");
            return View(model);
        }

        var kullanici = new Kullanici
        {
            Ad                   = model.Ad,
            Soyad                = model.Soyad,
            Eposta               = model.Eposta,
            Telefon              = model.Telefon,
            SifreHash            = null,  // şifre oluşturma maili ile belirlenecek
            AktiflikDurumu       = true,
            IkiFaktorluDogrulama = false,
            OlusturmaTarihi      = DateTime.Now
        };

        _context.Kullanicis.Add(kullanici);
        await _context.SaveChangesAsync();

        TempData["Basari"] = $"{kullanici.Ad} {kullanici.Soyad} adlı kullanıcı oluşturuldu.";
        return RedirectToAction(nameof(KullaniciOlustur));
    }
}
