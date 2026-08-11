using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;
using OBS.Services;
using OBS.ViewModels;
using System.Security.Claims;

namespace OBS.Controllers;

public class AuthController : Controller
{
    private readonly ObsContext _context;
    private readonly IEmailService _emailService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IPasswordSetupService _passwordSetupService;

    public AuthController(ObsContext context, IEmailService emailService,
        ITwoFactorService twoFactorService, IPasswordResetService passwordResetService,
        IPasswordSetupService passwordSetupService)
    {
        _context = context;
        _emailService = emailService;
        _twoFactorService = twoFactorService;
        _passwordResetService = passwordResetService;
        _passwordSetupService = passwordSetupService;
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
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        var kullanici = _context.Kullanicis
            .FirstOrDefault(k => k.Eposta == model.Eposta);

        if (kullanici == null)
        {
            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            return View(model);
        }

        // Şifresi henüz oluşturulmamış (admin tarafından eklendi, ilk giriş bekleniyor)
        if (kullanici.SifreHash == null)
        {
            ModelState.AddModelError(string.Empty, "Henüz şifreniz oluşturulmamış. Lütfen e-postanızı kontrol ediniz.");
            return View(model);
        }

        if (!BCrypt.Net.BCrypt.Verify(model.Sifre, kullanici.SifreHash))
        {
            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            return View(model);
        }

        if (!kullanici.AktiflikDurumu)
        {
            ModelState.AddModelError(string.Empty, "Hesabınız aktif değil. Lütfen yönetici ile iletişime geçin.");
            return View(model);
        }

        // 2FA aktif mi?
        if (kullanici.IkiFaktorluDogrulama)
        {
            // Kod üret ve mail gönder
            var code = _twoFactorService.GenerateCode(kullanici.Id);
            var body = Build2FAEmailBody($"{kullanici.Ad} {kullanici.Soyad}", code);

            try
            {
                await _emailService.SendAsync(kullanici.Eposta, $"{kullanici.Ad} {kullanici.Soyad}",
                    "OBS - Giriş Doğrulama Kodunuz", body);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Doğrulama kodu gönderilemedi. Lütfen daha sonra tekrar deneyiniz.");
                return View(model);
            }

            // Doğrulama ekranına yönlendir
            return RedirectToAction(nameof(Verify2FA), new { kullaniciId = kullanici.Id, returnUrl });
        }

        // 2FA kapalı → direkt oturum aç
        await SignInUserAsync(kullanici);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        // Admin ise onay ekranına yönlendir
        var isAdmin = await _context.KullaniciRols
            .Include(kr => kr.Rol)
            .AnyAsync(kr => kr.KullaniciId == kullanici.Id && kr.AktiflikDurumu && kr.Rol.RolAdi == "Admin");

        if (isAdmin)
            return RedirectToAction("KullaniciOlustur", "Admin");

        var isOgretimUyesi = await _context.OgretimUyesis
            .AnyAsync(ou => ou.KullaniciId == kullanici.Id);

        if (isOgretimUyesi)
            return RedirectToAction("Index", "Academician");

        return RedirectToAction("Index", "Student");
    }

    // GET: /Auth/Verify2FA
    [HttpGet]
    public IActionResult Verify2FA(int kullaniciId, string? returnUrl = null)
    {
        if (kullaniciId <= 0)
            return RedirectToAction(nameof(Login));

        var kullanici = _context.Kullanicis.Find(kullaniciId);
        if (kullanici == null)
            return RedirectToAction(nameof(Login));

        // E-postanın tamamını gösterme; sadece ilk 3 harf + *** + domain
        var eposta = kullanici.Eposta;
        var atIndex = eposta.IndexOf('@');
        var masked = atIndex > 3
            ? eposta[..3] + new string('*', atIndex - 3) + eposta[atIndex..]
            : eposta[..1] + "***" + eposta[atIndex..];

        ViewBag.MaskedEmail = masked;
        ViewBag.ReturnUrl   = returnUrl;

        return View(new VerifyViewModel { KullaniciId = kullaniciId });
    }

    // POST: /Auth/Verify2FA
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify2FA(VerifyViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            var k = _context.Kullanicis.Find(model.KullaniciId);
            if (k != null)
            {
                var eposta = k.Eposta;
                var atIndex = eposta.IndexOf('@');
                ViewBag.MaskedEmail = atIndex > 3
                    ? eposta[..3] + new string('*', atIndex - 3) + eposta[atIndex..]
                    : eposta[..1] + "***" + eposta[atIndex..];
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        var kullanici = _context.Kullanicis.Find(model.KullaniciId);
        if (kullanici == null)
            return RedirectToAction(nameof(Login));

        if (!_twoFactorService.ValidateCode(model.KullaniciId, model.Kod))
        {
            ModelState.AddModelError(nameof(model.Kod), "Kod hatalı veya süresi dolmuş. Lütfen tekrar giriş yapınız.");
            
            // Maskeli e-postayı tekrar ayarla
            var eposta  = kullanici.Eposta;
            var atIndex = eposta.IndexOf('@');
            ViewBag.MaskedEmail = atIndex > 3
                ? eposta[..3] + new string('*', atIndex - 3) + eposta[atIndex..]
                : eposta[..1] + "***" + eposta[atIndex..];
            ViewBag.ReturnUrl = returnUrl;

            return View(model);
        }

        await SignInUserAsync(kullanici);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    // POST: /Auth/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    // GET: /Auth/ForgotPassword
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    // POST: /Auth/ForgotPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Kullanıcı var mı? — güvenlik için her iki durumda da başarı mesajı göster
        var kullanici = _context.Kullanicis.FirstOrDefault(k => k.Eposta == model.Eposta);
        if (kullanici != null && kullanici.AktiflikDurumu)
        {
            var token   = _passwordResetService.GenerateToken(kullanici.Id);
            var resetUrl = Url.Action(nameof(ResetPassword), "Auth",
                new { token }, Request.Scheme)!;

            var body = BuildResetEmailBody($"{kullanici.Ad} {kullanici.Soyad}", resetUrl);

            try
            {
                await _emailService.SendAsync(kullanici.Eposta,
                    $"{kullanici.Ad} {kullanici.Soyad}",
                    "OBS - Şifre Sıfırlama", body);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Mail gönderilemedi. Lütfen daha sonra tekrar deneyiniz.");
                return View(model);
            }
        }

        // Her iki durumda da aynı mesaj (kullanıcı tespitini engeller)
        TempData["Basari"] = "Eğer bu e-posta sistemde kayıtlıysa, şifre sıfırlama linki gönderildi.";
        return RedirectToAction(nameof(ForgotPassword));
    }

    // GET: /Auth/ResetPassword?token=xxx
    [HttpGet]
    public IActionResult ResetPassword(string? token)
    {
        if (string.IsNullOrWhiteSpace(token) || _passwordResetService.GetKullaniciId(token) == null)
        {
            TempData["Hata"] = "Şifre sıfırlama linki geçersiz veya süresi dolmuş.";
            return RedirectToAction(nameof(Login));
        }

        return View(new ResetPasswordViewModel { Token = token });
    }

    // POST: /Auth/ResetPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var kullaniciId = _passwordResetService.GetKullaniciId(model.Token);
        if (kullaniciId == null)
        {
            ModelState.AddModelError(string.Empty, "Link geçersiz veya süresi dolmuş. Lütfen tekrar şifre sıfırlama isteği gönderin.");
            return View(model);
        }

        var kullanici = _context.Kullanicis.Find(kullaniciId.Value);
        if (kullanici == null)
            return RedirectToAction(nameof(Login));

        kullanici.SifreHash = BCrypt.Net.BCrypt.HashPassword(model.YeniSifre);
        kullanici.SonGuncellenmeTarihi = DateTime.Now;
        await _context.SaveChangesAsync();

        _passwordResetService.InvalidateToken(model.Token);

        TempData["Basari"] = "Şifreniz başarıyla güncellendi. Giriş yapabilirsiniz.";
        return RedirectToAction(nameof(Login));
    }

    // ── Yardımcı metotlar ────────────────────────────────────────────────────

    private async Task SignInUserAsync(Kullanici kullanici)
    {
        // Kullanıcının rollerini yükle
        var roller = await _context.KullaniciRols
            .Include(kr => kr.Rol)
            .Where(kr => kr.KullaniciId == kullanici.Id && kr.AktiflikDurumu)
            .ToListAsync();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
            new Claim(ClaimTypes.Name, $"{kullanici.Ad} {kullanici.Soyad}"),
            new Claim(ClaimTypes.Email, kullanici.Eposta)
        };

        // Rolleri claim olarak ekle
        foreach (var kr in roller)
        {
            claims.Add(new Claim(ClaimTypes.Role, kr.Rol.RolAdi));
        }

        bool isOgrenci = await _context.Ogrencis.AnyAsync(o => o.KullaniciId == kullanici.Id);
        if (isOgrenci)
        {
            claims.Add(new Claim("UserType", "Ogrenci"));
        }

        bool isOgretimUyesi = await _context.OgretimUyesis.AnyAsync(ou => ou.KullaniciId == kullanici.Id);
        if (isOgretimUyesi)
        {
            claims.Add(new Claim("UserType", "OgretimUyesi"));
        }

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

    private static string Build2FAEmailBody(string adSoyad, string code)
    {
        return $"""
        <!DOCTYPE html>
        <html lang="tr">
        <head><meta charset="utf-8"></head>
        <body style="font-family: Arial, sans-serif; background:#f0f2f5; margin:0; padding:20px;">
          <div style="max-width:480px; margin:0 auto; background:#fff; border-radius:10px; padding:32px; box-shadow:0 2px 16px rgba(0,0,0,.1);">
            <div style="text-align:center; margin-bottom:24px;">
              <span style="font-size:2rem;">📚</span>
              <h2 style="margin:8px 0 0; color:#2563eb; font-size:1.3rem;">OBS Sistemi</h2>
            </div>
            <p style="margin-bottom:8px;">Merhaba <strong>{adSoyad}</strong>,</p>
            <p style="color:#555; margin-bottom:24px;">
              Giriş doğrulama kodunuz aşağıdadır. Kod <strong>5 dakika</strong> geçerlidir.
            </p>
            <div style="background:#f0f4ff; border:2px dashed #2563eb; border-radius:8px; text-align:center; padding:20px 0; margin-bottom:24px;">
              <span style="font-size:2.5rem; font-weight:700; letter-spacing:0.35em; color:#2563eb;">{code}</span>
            </div>
            <p style="color:#888; font-size:0.85rem;">
              Bu kodu siz talep etmediyseniz lütfen bu e-postayı dikkate almayın ve şifrenizi değiştirin.
            </p>
            <hr style="border:none; border-top:1px solid #eee; margin:24px 0;">
            <p style="color:#aaa; font-size:0.75rem; text-align:center;">© OBS Sistemi — Otomatik gönderilmiştir, lütfen yanıtlamayın.</p>
          </div>
        </body>
        </html>
        """;
    }

    private static string BuildResetEmailBody(string adSoyad, string resetUrl)
    {
        return $"""
        <!DOCTYPE html>
        <html lang="tr">
        <head><meta charset="utf-8"></head>
        <body style="font-family: Arial, sans-serif; background:#f0f2f5; margin:0; padding:20px;">
          <div style="max-width:480px; margin:0 auto; background:#fff; border-radius:10px; padding:32px; box-shadow:0 2px 16px rgba(0,0,0,.1);">
            <div style="text-align:center; margin-bottom:24px;">
              <span style="font-size:2rem;">📚</span>
              <h2 style="margin:8px 0 0; color:#2563eb; font-size:1.3rem;">OBS Sistemi</h2>
            </div>
            <p style="margin-bottom:8px;">Merhaba <strong>{adSoyad}</strong>,</p>
            <p style="color:#555; margin-bottom:24px;">
              Şifre sıfırlama talebiniz alındı. Aşağıdaki butona tıklayarak yeni şifrenizi belirleyebilirsiniz.<br>
              Link <strong>15 dakika</strong> geçerlidir.
            </p>
            <div style="text-align:center; margin-bottom:24px;">
              <a href="{resetUrl}" style="display:inline-block; background:#2563eb; color:#fff; text-decoration:none; padding:12px 32px; border-radius:8px; font-weight:600; font-size:1rem;">
                Şifremi Sıfırla
              </a>
            </div>
            <p style="color:#888; font-size:0.85rem;">
              Bu talebi siz yapmadıysanız lütfen bu e-postayı dikkate almayın.
            </p>
            <hr style="border:none; border-top:1px solid #eee; margin:24px 0;">
            <p style="color:#aaa; font-size:0.75rem; text-align:center;">© OBS Sistemi — Otomatik gönderilmiştir, lütfen yanıtlamayın.</p>
          </div>
        </body>
        </html>
        """;
    }

    // GET: /Auth/SetPassword?token=xxx
    [HttpGet]
    public async Task<IActionResult> SetPassword(string token)
    {
        var kullaniciId = _passwordSetupService.GetKullaniciId(token);
        if (!kullaniciId.HasValue)
        {
            TempData["Hata"] = "Şifre oluşturma bağlantısının süresi dolmuş veya geçersiz.";
            return RedirectToAction(nameof(Login));
        }

        var kullanici = await _context.Kullanicis.FindAsync(kullaniciId.Value);
        if (kullanici == null)
        {
            TempData["Hata"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(Login));
        }

        var model = new SetPasswordViewModel
        {
            Token = token,
            AdSoyad = $"{kullanici.Ad} {kullanici.Soyad}",
            Eposta = kullanici.Eposta
        };

        return View(model);
    }

    // POST: /Auth/SetPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            if (string.IsNullOrEmpty(model.AdSoyad) || string.IsNullOrEmpty(model.Eposta))
            {
                var id = _passwordSetupService.GetKullaniciId(model.Token);
                if (id.HasValue)
                {
                    var k = await _context.Kullanicis.FindAsync(id.Value);
                    if (k != null)
                    {
                        model.AdSoyad = $"{k.Ad} {k.Soyad}";
                        model.Eposta = k.Eposta;
                    }
                }
            }
            return View(model);
        }

        var kullaniciId = _passwordSetupService.GetKullaniciId(model.Token);
        if (!kullaniciId.HasValue)
        {
            TempData["Hata"] = "Şifre oluşturma bağlantısının süresi dolmuş veya bağlantı daha önce kullanılmış.";
            return RedirectToAction(nameof(Login));
        }

        var kullanici = await _context.Kullanicis.FindAsync(kullaniciId.Value);
        if (kullanici == null)
        {
            TempData["Hata"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(Login));
        }

        // Şifreyi hashle ve kaydet
        kullanici.SifreHash = BCrypt.Net.BCrypt.HashPassword(model.Sifre);
        await _context.SaveChangesAsync();

        // Tokeni kullanıldı olarak işaretle (sil)
        _passwordSetupService.InvalidateToken(model.Token);

        TempData["Basari"] = "Tebrikler! Şifreniz başarıyla oluşturuldu. Artık sisteme giriş yapabilirsiniz.";
        return RedirectToAction(nameof(Login));
    }

    // GET: /Auth/FirstLogin
    [HttpGet]
    public IActionResult FirstLogin()
    {
        return View();
    }

    // POST: /Auth/FirstLogin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FirstLogin(string eposta)
    {
        if (string.IsNullOrWhiteSpace(eposta))
        {
            TempData["Hata"] = "Lütfen e-posta adresinizi giriniz.";
            return View();
        }

        eposta = eposta.Trim();
        var kullanici = await _context.Kullanicis.FirstOrDefaultAsync(k => k.Eposta == eposta);

        if (kullanici == null)
        {
            TempData["Hata"] = "Sistemde bu e-posta adresiyle kayıtlı bir hesap bulunamadı.";
            return View();
        }

        if (!string.IsNullOrEmpty(kullanici.SifreHash))
        {
            TempData["Hata"] = "Bu hesap için zaten bir şifre belirlenmiş. Şifrenizi unuttuysanız 'Şifremi Unuttum' ekranını kullanabilirsiniz.";
            return View();
        }

        // Token üret ve e-posta gönder
        var token = _passwordSetupService.GenerateToken(kullanici.Id);
        var setupLink = Url.Action("SetPassword", "Auth", new { token }, Request.Scheme);
        await _passwordSetupService.SendSetupEmailAsync(kullanici.Eposta, $"{kullanici.Ad} {kullanici.Soyad}", setupLink!);

        TempData["Basari"] = $"Şifre oluşturma bağlantısı {kullanici.Eposta} adresine gönderildi. Lütfen e-postanızı kontrol ediniz. (Test Linki: {setupLink})";
        return RedirectToAction(nameof(Login));
    }
}
