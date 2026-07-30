using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OBS.Models;
using OBS.Services;
using OBS.ViewModels;

namespace OBS.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ObsContext _context;
    private readonly IPasswordSetupService _passwordSetupService;

    public AdminController(ObsContext context, IPasswordSetupService passwordSetupService)
    {
        _context = context;
        _passwordSetupService = passwordSetupService;
    }

    // GET: /Admin/KullaniciOlustur
    [HttpGet]
    public async Task<IActionResult> KullaniciOlustur()
    {
        await DoldurSelectListler();
        return View(new KullaniciOlusturViewModel());
    }

    // POST: /Admin/KullaniciOlustur
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KullaniciOlustur(KullaniciOlusturViewModel model)
    {
        // Tipe özel manuel validasyon kontrolleri
        if (model.KullaniciTipi == "Ogrenci")
        {
            if (string.IsNullOrWhiteSpace(model.Cinsiyet))
                ModelState.AddModelError(nameof(model.Cinsiyet), "Öğrenci için cinsiyet seçimi zorunludur.");
            if (!model.OrganizasyonId.HasValue || model.OrganizasyonId <= 0)
                ModelState.AddModelError(nameof(model.OrganizasyonId), "Öğrenci için organizasyon / bölüm seçimi zorunludur.");
            if (string.IsNullOrWhiteSpace(model.OgrenciNo))
                ModelState.AddModelError(nameof(model.OgrenciNo), "Öğrenci numarası zorunludur.");

            if (!model.GirisTarihi.HasValue)
                ModelState.AddModelError(nameof(model.GirisTarihi), "Giriş tarihi zorunludur.");
            if (string.IsNullOrWhiteSpace(model.OgrenciTipi))
                ModelState.AddModelError(nameof(model.OgrenciTipi), "Öğrenci tipi seçimi zorunludur.");
            if (string.IsNullOrWhiteSpace(model.OgrenciDurum))
                ModelState.AddModelError(nameof(model.OgrenciDurum), "Öğrenci durumu seçimi zorunludur.");
            if (!model.Sinif.HasValue || model.Sinif <= 0)
                ModelState.AddModelError(nameof(model.Sinif), "Sınıf bilgisi zorunludur.");

            if (!string.IsNullOrWhiteSpace(model.OgrenciNo) &&
                await _context.Ogrencis.AnyAsync(o => o.OgrenciNo == model.OgrenciNo))
            {
                ModelState.AddModelError(nameof(model.OgrenciNo), "Bu öğrenci numarası sistemde zaten kayıtlı.");
            }
        }
        else if (model.KullaniciTipi == "OgretimUyesi")
        {
            if (string.IsNullOrWhiteSpace(model.Cinsiyet))
                ModelState.AddModelError(nameof(model.Cinsiyet), "Öğretim üyesi için cinsiyet seçimi zorunludur.");
            if (!model.OrganizasyonId.HasValue || model.OrganizasyonId <= 0)
                ModelState.AddModelError(nameof(model.OrganizasyonId), "Öğretim üyesi için organizasyon / bölüm seçimi zorunludur.");
            if (!model.GorevBaslangic.HasValue)
                ModelState.AddModelError(nameof(model.GorevBaslangic), "Görev başlangıç tarihi zorunludur.");
        }

        if (model.OrganizasyonId.HasValue && model.OrganizasyonId > 0)
        {
            var secilenOrg = await _context.Organizasyons.FindAsync(model.OrganizasyonId.Value);
            if (secilenOrg == null || secilenOrg.UstOrganizasyonId == null)
            {
                ModelState.AddModelError(nameof(model.OrganizasyonId), "Lütfen sadece geçerli bir bölüm seçiniz (Fakülte seçilemez).");
            }
        }

        if (!ModelState.IsValid)
        {
            await DoldurSelectListler();
            return View(model);
        }

        // E-posta benzersizlik kontrolü
        if (await _context.Kullanicis.AnyAsync(k => k.Eposta == model.Eposta))
        {
            ModelState.AddModelError("Eposta", "Bu e-posta adresi zaten kayıtlı.");
            await DoldurSelectListler();
            return View(model);
        }

        // 1) Kullanıcı kaydı oluştur
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
        await _context.SaveChangesAsync(); // PK Id üretildi

        // 2) Eğer Öğrenci veya Öğretim Üyesi seçildiyse ilgili tabloya kayıt ekle
        if (model.KullaniciTipi == "Ogrenci")
        {
            var ogrenci = new Ogrenci
            {
                KullaniciId    = kullanici.Id,
                Cinsiyet       = model.Cinsiyet!,
                OgrenciNo      = model.OgrenciNo!,
                DanismanId     = null,
                OrganizasyonId = model.OrganizasyonId!.Value,
                GirisTarihi    = model.GirisTarihi!.Value,
                OgrenciTipi    = model.OgrenciTipi!,
                Durum          = model.OgrenciDurum!,
                Sinif          = model.Sinif!.Value
            };
            _context.Ogrencis.Add(ogrenci);
            await _context.SaveChangesAsync();
        }
        else if (model.KullaniciTipi == "OgretimUyesi")
        {
            var ogretimUyesi = new OgretimUyesi
            {
                KullaniciId    = kullanici.Id,
                Cinsiyet       = model.Cinsiyet!,
                Unvan          = string.IsNullOrWhiteSpace(model.Unvan) ? null : model.Unvan,
                OrganizasyonId = model.OrganizasyonId!.Value,
                KadroTipi      = string.IsNullOrWhiteSpace(model.KadroTipi) ? null : model.KadroTipi,
                GorevBaslangic = model.GorevBaslangic!.Value,
                GorevBitis     = model.GorevBitis
            };
            _context.OgretimUyesis.Add(ogretimUyesi);
            await _context.SaveChangesAsync();
        }

        // 3) Şifre oluşturma (setup) tokeni üret ve e-posta gönder
        var token = _passwordSetupService.GenerateToken(kullanici.Id);
        var setupLink = Url.Action("SetPassword", "Auth", new { token }, Request.Scheme);
        await _passwordSetupService.SendSetupEmailAsync(kullanici.Eposta, $"{kullanici.Ad} {kullanici.Soyad}", setupLink!);

        var tipMesaj = model.KullaniciTipi == "Ogrenci" ? " (Öğrenci olarak)" :
                       model.KullaniciTipi == "OgretimUyesi" ? " (Öğretim Üyesi olarak)" : "";

        TempData["Basari"] = $"{kullanici.Ad} {kullanici.Soyad} adlı kullanıcı{tipMesaj} başarıyla oluşturuldu! Kurulum e-postası ({kullanici.Eposta}) adresine gönderildi. Test Linki: {setupLink}";
        return RedirectToAction(nameof(KullaniciOlustur));
    }

    private async Task DoldurSelectListler()
    {
        var tumOrganizasyonlar = await _context.Organizasyons
            .Where(o => o.Durum)
            .OrderBy(o => o.Adi)
            .ToListAsync();

        // Üst organizasyonlar (Fakülteler / Enstitüler / Yüksekokullar)
        var fakulteler = tumOrganizasyonlar
            .Where(o => o.UstOrganizasyonId == null || tumOrganizasyonlar.Any(alt => alt.UstOrganizasyonId == o.Id))
            .ToList();

        // Alt organizasyonlar (Fakülteye bağlı bölümler / programlar)
        var bolumler = tumOrganizasyonlar
            .Where(o => o.UstOrganizasyonId != null)
            .Select(o => new
            {
                o.Id,
                o.Adi,
                o.UstOrganizasyonId
            })
            .ToList();

        ViewBag.Fakulteler = new SelectList(fakulteler, "Id", "Adi");
        ViewBag.BolumlerListesi = bolumler;
    }

    // GET: /Admin/DanismanAtama
    [HttpGet]
    public async Task<IActionResult> DanismanAtama(int? bolumId, string? arama)
    {
        var query = _context.Ogrencis
            .Include(o => o.Kullanici)
            .Include(o => o.Organizasyon)
            .Include(o => o.Danisman)
                .ThenInclude(d => d!.Kullanici)
            .AsQueryable();

        if (bolumId.HasValue && bolumId.Value > 0)
        {
            query = query.Where(o => o.OrganizasyonId == bolumId.Value);
        }

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim().ToLower();
            query = query.Where(o => o.OgrenciNo.ToLower().Contains(arama) ||
                                     o.Kullanici.Ad.ToLower().Contains(arama) ||
                                     o.Kullanici.Soyad.ToLower().Contains(arama));
        }

        var ogrenciler = await query
            .OrderBy(o => o.Organizasyon.Adi)
            .ThenBy(o => o.Sinif)
            .ThenBy(o => o.OgrenciNo)
            .ToListAsync();

        var model = ogrenciler.Select(o => new DanismanAtamaViewModel
        {
            OgrenciId = o.Id,
            OgrenciNo = o.OgrenciNo,
            AdSoyad = $"{o.Kullanici.Ad} {o.Kullanici.Soyad}",
            BolumAdi = o.Organizasyon.Adi,
            Sinif = o.Sinif,
            DanismanId = o.DanismanId,
            DanismanAdSoyad = o.Danisman != null ? $"{o.Danisman.Unvan} {o.Danisman.Kullanici.Ad} {o.Danisman.Kullanici.Soyad}".Trim() : null
        }).ToList();

        // Bölüm listesi (filtreleme için)
        var bolumler = await _context.Organizasyons
            .Where(o => o.Durum && o.UstOrganizasyonId != null)
            .OrderBy(o => o.Adi)
            .ToListAsync();
        ViewBag.Bolumler = new SelectList(bolumler, "Id", "Adi", bolumId);

        // Öğretim Üyeleri listesi
        var hocalar = await _context.OgretimUyesis
            .Include(ou => ou.Kullanici)
            .Include(ou => ou.Organizasyon)
            .OrderBy(ou => ou.Organizasyon.Adi)
            .ThenBy(ou => ou.Kullanici.Ad)
            .ToListAsync();

        ViewBag.HocaListesi = hocalar.Select(h => new
        {
            h.Id,
            AdSoyad = $"{h.Unvan} {h.Kullanici.Ad} {h.Kullanici.Soyad}".Trim(),
            BolumAdi = h.Organizasyon.Adi
        }).ToList();

        ViewBag.SeciliBolumId = bolumId;
        ViewBag.Arama = arama;

        return View(model);
    }

    // POST: /Admin/TopluDanismanAta
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TopluDanismanAta(List<int> ogrenciId, List<int?> danismanId, int? bolumId, string? arama)
    {
        if (ogrenciId == null || !ogrenciId.Any())
        {
            TempData["Hata"] = "Güncellenecek öğrenci bulunamadı.";
            return RedirectToAction(nameof(DanismanAtama), new { bolumId, arama });
        }

        var ogrenciler = await _context.Ogrencis
            .Where(o => ogrenciId.Contains(o.Id))
            .ToListAsync();

        int guncellenenSayisi = 0;
        for (int i = 0; i < ogrenciId.Count; i++)
        {
            var id = ogrenciId[i];
            var dId = (danismanId != null && i < danismanId.Count) ? danismanId[i] : null;

            var ogrenci = ogrenciler.FirstOrDefault(o => o.Id == id);
            if (ogrenci != null)
            {
                var yeniDanismanId = (dId.HasValue && dId.Value > 0) ? dId.Value : (int?)null;
                if (ogrenci.DanismanId != yeniDanismanId)
                {
                    ogrenci.DanismanId = yeniDanismanId;
                    guncellenenSayisi++;
                }
            }
        }

        if (guncellenenSayisi > 0)
        {
            await _context.SaveChangesAsync();
            TempData["Basari"] = $"{guncellenenSayisi} öğrencinin danışman bilgisi başarıyla güncellendi!";
        }
        else
        {
            TempData["Basari"] = "Herhangi bir değişiklik yapılmadı.";
        }

        return RedirectToAction(nameof(DanismanAtama), new { bolumId, arama });
    }

    // GET: /Admin/RolYetkiYonetimi
    [HttpGet]
    public async Task<IActionResult> RolYetkiYonetimi()
    {
        var roller = await _context.Rols.OrderBy(r => r.RolAdi).ToListAsync();
        var yetkiler = await _context.Yetkis.OrderBy(y => y.YetkiKodu).ToListAsync();
        var rolYetkiler = await _context.RolYetkis.ToListAsync();

        var model = new RolYetkiYonetimiViewModel
        {
            Roller = roller,
            Yetkiler = yetkiler,
            RolYetkiler = rolYetkiler
        };

        return View(model);
    }

    // POST: /Admin/RolEkle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RolEkle(RolEkleModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Hata"] = "Rol adı zorunludur ve kurallara uymalıdır.";
            return RedirectToAction(nameof(RolYetkiYonetimi));
        }

        model.RolAdi = model.RolAdi.Trim();
        if (await _context.Rols.AnyAsync(r => r.RolAdi.ToLower() == model.RolAdi.ToLower()))
        {
            TempData["Hata"] = $"'{model.RolAdi}' adında bir rol sistemde zaten mevcut.";
            return RedirectToAction(nameof(RolYetkiYonetimi));
        }

        var rol = new Rol
        {
            RolAdi = model.RolAdi,
            Aciklama = model.Aciklama?.Trim()
        };

        _context.Rols.Add(rol);
        await _context.SaveChangesAsync();

        TempData["Basari"] = $"'{rol.RolAdi}' rolü başarıyla oluşturuldu.";
        return RedirectToAction(nameof(RolYetkiYonetimi));
    }

    // POST: /Admin/RolSil
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RolSil(int id)
    {
        var rol = await _context.Rols
            .Include(r => r.KullaniciRols)
            .Include(r => r.RolYetkis)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (rol == null)
        {
            TempData["Hata"] = "Silinecek rol bulunamadı.";
            return RedirectToAction(nameof(RolYetkiYonetimi));
        }

        // Sistem için kritik rolleri silmeyi engelle
        var korunanRoller = new[] { "Admin", "Ogrenci", "OgretimUyesi" };
        if (korunanRoller.Contains(rol.RolAdi, StringComparer.OrdinalIgnoreCase))
        {
            TempData["Hata"] = $"'{rol.RolAdi}' sistem için zorunlu bir roldür ve silinemez!";
            return RedirectToAction(nameof(RolYetkiYonetimi));
        }

        _context.KullaniciRols.RemoveRange(rol.KullaniciRols);
        _context.RolYetkis.RemoveRange(rol.RolYetkis);
        _context.Rols.Remove(rol);
        await _context.SaveChangesAsync();

        TempData["Basari"] = $"'{rol.RolAdi}' rolü ve ilişkili tüm atamalar silindi.";
        return RedirectToAction(nameof(RolYetkiYonetimi));
    }

    // POST: /Admin/YetkiEkle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> YetkiEkle(YetkiEkleModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Hata"] = "Yetki kodu zorunludur ve sadece büyük harf/rakam/_ içerebilir.";
            return RedirectToAction(nameof(RolYetkiYonetimi));
        }

        model.YetkiKodu = model.YetkiKodu.Trim().ToUpper();
        if (await _context.Yetkis.AnyAsync(y => y.YetkiKodu == model.YetkiKodu))
        {
            TempData["Hata"] = $"'{model.YetkiKodu}' kodlu yetki zaten mevcut.";
            return RedirectToAction(nameof(RolYetkiYonetimi));
        }

        var yetki = new Yetki
        {
            YetkiKodu = model.YetkiKodu,
            Aciklama = model.Aciklama?.Trim()
        };

        _context.Yetkis.Add(yetki);
        await _context.SaveChangesAsync();

        TempData["Basari"] = $"'{yetki.YetkiKodu}' yetkisi başarıyla tanımlandı.";
        return RedirectToAction(nameof(RolYetkiYonetimi));
    }

    // POST: /Admin/YetkiSil
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> YetkiSil(int id)
    {
        var yetki = await _context.Yetkis
            .Include(y => y.RolYetkis)
            .Include(y => y.KullaniciYetkis)
            .FirstOrDefaultAsync(y => y.Id == id);

        if (yetki == null)
        {
            TempData["Hata"] = "Silinecek yetki bulunamadı.";
            return RedirectToAction(nameof(RolYetkiYonetimi));
        }

        _context.RolYetkis.RemoveRange(yetki.RolYetkis);
        _context.KullaniciYetkis.RemoveRange(yetki.KullaniciYetkis);
        _context.Yetkis.Remove(yetki);
        await _context.SaveChangesAsync();

        TempData["Basari"] = $"'{yetki.YetkiKodu}' yetkisi ve ilişkili tüm atamalar silindi.";
        return RedirectToAction(nameof(RolYetkiYonetimi));
    }

    // POST: /Admin/RolYetkiKaydet
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RolYetkiKaydet(RolYetkiAtamaModel model)
    {
        var rol = await _context.Rols.Include(r => r.RolYetkis).FirstOrDefaultAsync(r => r.Id == model.RolId);
        if (rol == null)
        {
            TempData["Hata"] = "Seçilen rol bulunamadı.";
            return RedirectToAction(nameof(RolYetkiYonetimi));
        }

        // Mevcut yetkileri kaldır
        _context.RolYetkis.RemoveRange(rol.RolYetkis);

        // Yeni yetkileri ekle
        if (model.SeciliYetkiIdler != null && model.SeciliYetkiIdler.Any())
        {
            foreach (var yId in model.SeciliYetkiIdler.Distinct())
            {
                _context.RolYetkis.Add(new RolYetki
                {
                    RolId = rol.Id,
                    YetkiId = yId,
                    BaslangicTarihi = DateTime.Now
                });
            }
        }

        await _context.SaveChangesAsync();
        TempData["Basari"] = $"'{rol.RolAdi}' rolünün yetkileri güncellendi!";
        return RedirectToAction(nameof(RolYetkiYonetimi));
    }

    // GET: /Admin/KullaniciYetkilendirme
    [HttpGet]
    public async Task<IActionResult> KullaniciYetkilendirme(string? arama, int? bolumId)
    {
        var query = _context.Kullanicis
            .Include(k => k.KullaniciRols.Where(kr => kr.AktiflikDurumu))
                .ThenInclude(kr => kr.Rol)
            .Include(k => k.KullaniciYetkiKullanicis)
                .ThenInclude(ky => ky.Yetki)
            .Where(k => k.AktiflikDurumu)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim().ToLower();
            query = query.Where(k => k.Ad.ToLower().Contains(arama) ||
                                     k.Soyad.ToLower().Contains(arama) ||
                                     k.Eposta.ToLower().Contains(arama));
        }

        var kullanicilar = await query.OrderBy(k => k.Ad).ThenBy(k => k.Soyad).ToListAsync();

        var ogrenciler = await _context.Ogrencis.Include(o => o.Organizasyon).ToListAsync();
        var hocalar = await _context.OgretimUyesis.Include(ou => ou.Organizasyon).ToListAsync();

        var liste = new List<KullaniciYetkiOzetModel>();
        foreach (var k in kullanicilar)
        {
            var ogrenci = ogrenciler.FirstOrDefault(o => o.KullaniciId == k.Id);
            var hoca = hocalar.FirstOrDefault(h => h.KullaniciId == k.Id);

            int? orgId = ogrenci?.OrganizasyonId ?? hoca?.OrganizasyonId;
            if (bolumId.HasValue && bolumId.Value > 0 && orgId != bolumId.Value)
                continue;

            liste.Add(new KullaniciYetkiOzetModel
            {
                KullaniciId = k.Id,
                AdSoyad = $"{k.Ad} {k.Soyad}",
                Eposta = k.Eposta,
                BolumAdi = ogrenci?.Organizasyon?.Adi ?? hoca?.Organizasyon?.Adi ?? "-",
                KullaniciTipi = ogrenci != null ? "Öğrenci" : hoca != null ? "Öğretim Üyesi" : "Yönetici / Personel",
                Roller = k.KullaniciRols.Select(kr => kr.Rol.RolAdi).ToList(),
                RolIdler = k.KullaniciRols.Select(kr => kr.RolId).ToList(),
                DogrudanYetkiler = k.KullaniciYetkiKullanicis.Select(ky => ky.Yetki.YetkiKodu).ToList(),
                DogrudanYetkiIdler = k.KullaniciYetkiKullanicis.Select(ky => ky.YetkiId).ToList()
            });
        }

        var bolumler = await _context.Organizasyons
            .Where(o => o.Durum && o.UstOrganizasyonId != null)
            .OrderBy(o => o.Adi)
            .ToListAsync();

        var tumRoller = await _context.Rols.OrderBy(r => r.RolAdi).ToListAsync();
        var tumYetkiler = await _context.Yetkis.OrderBy(y => y.YetkiKodu).ToListAsync();

        var model = new KullaniciYetkilendirmeViewModel
        {
            Kullanicilar = liste,
            TumRoller = tumRoller,
            TumYetkiler = tumYetkiler,
            Arama = arama,
            BolumId = bolumId
        };

        ViewBag.Bolumler = new SelectList(bolumler, "Id", "Adi", bolumId);
        return View(model);
    }

    // POST: /Admin/KullaniciYetkilendirmeKaydet
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KullaniciYetkilendirmeKaydet(KullaniciYetkilendirmeKaydetModel model)
    {
        var kullanici = await _context.Kullanicis
            .Include(k => k.KullaniciRols)
            .Include(k => k.KullaniciYetkiKullanicis)
            .FirstOrDefaultAsync(k => k.Id == model.KullaniciId);

        if (kullanici == null)
        {
            TempData["Hata"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(KullaniciYetkilendirme), new { arama = model.Arama, bolumId = model.BolumId });
        }

        // 1) Mevcut rolleri güncelle / sil / ekle
        _context.KullaniciRols.RemoveRange(kullanici.KullaniciRols);
        if (model.SeciliRolIdler != null && model.SeciliRolIdler.Any())
        {
            foreach (var rId in model.SeciliRolIdler.Distinct())
            {
                _context.KullaniciRols.Add(new KullaniciRol
                {
                    KullaniciId = kullanici.Id,
                    RolId = rId,
                    AktiflikDurumu = true,
                    BaslangicTarihi = DateTime.Now
                });
            }
        }

        // 2) Mevcut doğrudan yetkileri güncelle / sil / ekle
        _context.KullaniciYetkis.RemoveRange(kullanici.KullaniciYetkiKullanicis);
        if (model.SeciliYetkiIdler != null && model.SeciliYetkiIdler.Any())
        {
            int? islemYapanId = null;
            if (int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out int uId))
            {
                islemYapanId = uId;
            }

            foreach (var yId in model.SeciliYetkiIdler.Distinct())
            {
                _context.KullaniciYetkis.Add(new KullaniciYetki
                {
                    KullaniciId = kullanici.Id,
                    YetkiId = yId,
                    IslemYapanKullaniciId = islemYapanId,
                    BaslangicTarihi = DateTime.Now
                });
            }
        }

        await _context.SaveChangesAsync();
        TempData["Basari"] = $"{kullanici.Ad} {kullanici.Soyad} kullanıcısının rol ve yetkileri başarıyla güncellendi!";
        return RedirectToAction(nameof(KullaniciYetkilendirme), new { arama = model.Arama, bolumId = model.BolumId });
    }
}
