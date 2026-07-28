using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

namespace OBS.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ObsContext _context;

    public AdminController(ObsContext context)
    {
        _context = context;
    }

    // GET: /Admin/BekleyenKullanicilar
    public async Task<IActionResult> BekleyenKullanicilar()
    {
        var bekleyenler = await _context.Kullanicis
            .Where(k => !k.AktiflikDurumu)
            .OrderBy(k => k.OlusturmaTarihi)
            .ToListAsync();

        return View(bekleyenler);
    }

    // POST: /Admin/KullaniciOnayla/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KullaniciOnayla(int id)
    {
        var kullanici = await _context.Kullanicis.FindAsync(id);
        if (kullanici == null)
        {
            TempData["Hata"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(BekleyenKullanicilar));
        }

        kullanici.AktiflikDurumu = true;
        kullanici.SonGuncellenmeTarihi = DateTime.Now;
        await _context.SaveChangesAsync();

        TempData["Basari"] = $"{kullanici.Ad} {kullanici.Soyad} adlı kullanıcı onaylandı.";
        return RedirectToAction(nameof(BekleyenKullanicilar));
    }

    // POST: /Admin/KullaniciReddet/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KullaniciReddet(int id)
    {
        var kullanici = await _context.Kullanicis.FindAsync(id);
        if (kullanici == null)
        {
            TempData["Hata"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(BekleyenKullanicilar));
        }

        _context.Kullanicis.Remove(kullanici);
        await _context.SaveChangesAsync();

        TempData["Basari"] = $"Kullanıcı kaydı silindi.";
        return RedirectToAction(nameof(BekleyenKullanicilar));
    }
}
