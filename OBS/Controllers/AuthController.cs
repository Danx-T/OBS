using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OBS.Models;
using OBS.ViewModels;
using System.Security.Claims;

namespace OBS.Controllers;

public class AuthController : Controller
{
    private readonly ObsContext _context;

    public AuthController(ObsContext context)
    {
        _context = context;
    }

    // GET: /Auth/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // POST: /Auth/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var kullanici = _context.Kullanicis
            .FirstOrDefault(k => k.Eposta == model.Eposta);

        if (kullanici == null || !BCrypt.Net.BCrypt.Verify(model.Sifre, kullanici.SifreHash))
        {
            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            return View(model);
        }

        if (!kullanici.AktiflikDurumu)
        {
            ModelState.AddModelError(string.Empty, "Hesabınız aktif değil. Lütfen yönetici ile iletişime geçin.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
            new Claim(ClaimTypes.Name, $"{kullanici.Ad} {kullanici.Soyad}"),
            new Claim(ClaimTypes.Email, kullanici.Eposta)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    // GET: /Auth/Register
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View();
    }

    // POST: /Auth/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // E-posta benzersizlik kontrolü
        if (_context.Kullanicis.Any(k => k.Eposta == model.Eposta))
        {
            ModelState.AddModelError("Eposta", "Bu e-posta adresi zaten kayıtlı.");
            return View(model);
        }

        var kullanici = new Kullanici
        {
            Ad = model.Ad,
            Soyad = model.Soyad,
            Eposta = model.Eposta,
            Telefon = model.Telefon,
            SifreHash = BCrypt.Net.BCrypt.HashPassword(model.Sifre),
            AktiflikDurumu = true,
            IkiFaktorluDogrulama = false,
            OlusturmaTarihi = DateTime.Now
        };

        _context.Kullanicis.Add(kullanici);
        await _context.SaveChangesAsync();

        TempData["Basari"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
        return RedirectToAction(nameof(Login));
    }

    // POST: /Auth/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
