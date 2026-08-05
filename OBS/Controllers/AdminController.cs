using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OBS.Models;
using OBS.Services;
using OBS.ViewModels;
using System.Globalization;

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
            Ad                   = model.Ad.Trim(),
            Soyad                = model.Soyad.Trim(),
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

        // Öğretim Üyeleri listesi - sadece 'Danışman' rolü atanmış olanlar
        var hocalar = await _context.OgretimUyesis
            .Include(ou => ou.Kullanici)
                .ThenInclude(k => k.KullaniciRols)
                    .ThenInclude(kr => kr.Rol)
            .Include(ou => ou.Organizasyon)
            .Where(ou => ou.Kullanici.KullaniciRols.Any(kr => kr.Rol.RolAdi.Contains("Danışman") || kr.Rol.RolAdi.Contains("Danisman")))
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

        var gecerliDanismanIdler = await _context.OgretimUyesis
            .Where(ou => ou.Kullanici.KullaniciRols.Any(kr => kr.Rol.RolAdi.Contains("Danışman") || kr.Rol.RolAdi.Contains("Danisman")))
            .Select(ou => ou.Id)
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
                if (yeniDanismanId.HasValue && !gecerliDanismanIdler.Contains(yeniDanismanId.Value))
                {
                    continue; // Danışman rolü olmayan bir öğretim üyesi atanamaz
                }

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
                KullaniciTipi = ogrenci != null ? "Öğrenci" : hoca != null ? "Öğretim Üyesi" : "Personel",
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

    // ─────────────────────────────────────────────────────────────────────────
    // KULLANICI YÖNETİM PANELİ
    // ─────────────────────────────────────────────────────────────────────────

    // GET: /Admin/KullaniciYonetimi
    [HttpGet]
    public async Task<IActionResult> KullaniciYonetimi(string? arama, string? durumFiltre, string? tipFiltre)
    {
        var query = _context.Kullanicis
            .Include(k => k.KullaniciRols.Where(kr => kr.AktiflikDurumu))
                .ThenInclude(kr => kr.Rol)
            .Include(k => k.KullaniciYetkiKullanicis)
                .ThenInclude(ky => ky.Yetki)
            .Include(k => k.Ogrenci)
                .ThenInclude(o => o!.Organizasyon)
            .Include(k => k.OgretimUyesi)
                .ThenInclude(ou => ou!.Organizasyon)
            .AsQueryable();

        // Arama filtresi
        if (!string.IsNullOrWhiteSpace(arama))
        {
            var aramaLower = arama.Trim().ToLower();
            query = query.Where(k => k.Ad.ToLower().Contains(aramaLower) ||
                                     k.Soyad.ToLower().Contains(aramaLower) ||
                                     k.Eposta.ToLower().Contains(aramaLower));
        }

        // Durum filtresi
        if (durumFiltre == "aktif")
            query = query.Where(k => k.AktiflikDurumu);
        else if (durumFiltre == "pasif")
            query = query.Where(k => !k.AktiflikDurumu);

        var kullanicilar = await query.OrderBy(k => k.Ad).ThenBy(k => k.Soyad).ToListAsync();

        var liste = kullanicilar.Select(k =>
        {
            var tip = k.Ogrenci != null ? "Öğrenci"
                    : k.OgretimUyesi != null ? "Öğretim Üyesi"
                    : "Personel";

            var bolum = k.Ogrenci?.Organizasyon?.Adi
                     ?? k.OgretimUyesi?.Organizasyon?.Adi;

            return new OBS.ViewModels.KullaniciYonetimListeModel
            {
                Id                    = k.Id,
                AdSoyad               = $"{k.Ad} {k.Soyad}",
                Eposta                = k.Eposta,
                Telefon               = k.Telefon,
                AktiflikDurumu        = k.AktiflikDurumu,
                KullaniciTipi         = tip,
                BolumAdi              = bolum,
                OlusturmaTarihi       = k.OlusturmaTarihi,
                SonGuncellenmeTarihi  = k.SonGuncellenmeTarihi,
                IkiFaktorluDogrulama  = k.IkiFaktorluDogrulama,
                SifreBelirlenmisMi    = !string.IsNullOrEmpty(k.SifreHash),
                Roller                = k.KullaniciRols.Select(kr => kr.Rol.RolAdi).ToList(),
                Yetkiler              = k.KullaniciYetkiKullanicis.Select(ky => ky.Yetki.YetkiKodu).ToList()
            };
        }).ToList();

        // Tip filtresi (client-side kolayca yapılabilir ama server-side da destekleyelim)
        if (!string.IsNullOrWhiteSpace(tipFiltre))
        {
            liste = tipFiltre switch
            {
                "Ogrenci"       => liste.Where(x => x.KullaniciTipi == "Öğrenci").ToList(),
                "OgretimUyesi"  => liste.Where(x => x.KullaniciTipi == "Öğretim Üyesi").ToList(),
                "Diger"         => liste.Where(x => x.KullaniciTipi == "Personel").ToList(),
                _               => liste
            };
        }

        var model = new OBS.ViewModels.KullaniciYonetimViewModel
        {
            Kullanicilar  = liste,
            Arama         = arama,
            DurumFiltre   = durumFiltre,
            TipFiltre     = tipFiltre
        };

        return View(model);
    }

    // POST: /Admin/KullaniciAktiflikToggle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KullaniciAktiflikToggle(int id, string? arama, string? durumFiltre, string? tipFiltre)
    {
        // Kendi hesabını pasife alamaz
        if (int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out int mevcutId)
            && mevcutId == id)
        {
            TempData["Hata"] = "Kendi hesabınızın aktiflik durumunu değiştiremezsiniz!";
            return RedirectToAction(nameof(KullaniciYonetimi), new { arama, durumFiltre, tipFiltre });
        }

        var kullanici = await _context.Kullanicis.FindAsync(id);
        if (kullanici == null)
        {
            TempData["Hata"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(KullaniciYonetimi), new { arama, durumFiltre, tipFiltre });
        }

        // Tek admin kontrolü: pasife geçirilecekse ve admin rolündeyse başka admin var mı?
        if (kullanici.AktiflikDurumu)
        {
            var adminRolId = await _context.Rols
                .Where(r => r.RolAdi == "Admin")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (adminRolId > 0)
            {
                var adminKullaniciBuKisi = await _context.KullaniciRols
                    .AnyAsync(kr => kr.KullaniciId == id && kr.RolId == adminRolId && kr.AktiflikDurumu);

                if (adminKullaniciBuKisi)
                {
                    var diger_aktif_admin = await _context.KullaniciRols
                        .CountAsync(kr => kr.RolId == adminRolId && kr.AktiflikDurumu && kr.KullaniciId != id
                                         && kr.Kullanici.AktiflikDurumu);
                    if (diger_aktif_admin == 0)
                    {
                        TempData["Hata"] = "Sistemde tek aktif admin bu kullanıcıdır. Önce başka bir kullanıcıya Admin rolü atayınız.";
                        return RedirectToAction(nameof(KullaniciYonetimi), new { arama, durumFiltre, tipFiltre });
                    }
                }
            }
        }

        kullanici.AktiflikDurumu = !kullanici.AktiflikDurumu;
        kullanici.SonGuncellenmeTarihi = DateTime.Now;
        await _context.SaveChangesAsync();

        var durum = kullanici.AktiflikDurumu ? "aktif" : "pasif";
        TempData["Basari"] = $"{kullanici.Ad} {kullanici.Soyad} kullanıcısı başarıyla {durum} yapıldı.";
        return RedirectToAction(nameof(KullaniciYonetimi), new { arama, durumFiltre, tipFiltre });
    }

    // POST: /Admin/KullaniciSil
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KullaniciSil(int id, string? arama, string? durumFiltre, string? tipFiltre)
    {
        // Kendi hesabını silemez
        if (int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out int mevcutId)
            && mevcutId == id)
        {
            TempData["Hata"] = "Kendi hesabınızı silemezsiniz!";
            return RedirectToAction(nameof(KullaniciYonetimi), new { arama, durumFiltre, tipFiltre });
        }

        var kullanici = await _context.Kullanicis
            .Include(k => k.KullaniciRols)
            .Include(k => k.KullaniciYetkiKullanicis)
            .Include(k => k.KullaniciYetkiIslemYapanKullanicis)
            .Include(k => k.DenetimKaydis)
            .Include(k => k.Ogrenci)
            .Include(k => k.OgretimUyesi)
            .FirstOrDefaultAsync(k => k.Id == id);

        if (kullanici == null)
        {
            TempData["Hata"] = "Silinecek kullanıcı bulunamadı.";
            return RedirectToAction(nameof(KullaniciYonetimi), new { arama, durumFiltre, tipFiltre });
        }

        // Tek admin kontrolü
        var adminRolId = await _context.Rols
            .Where(r => r.RolAdi == "Admin")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        if (adminRolId > 0 && kullanici.KullaniciRols.Any(kr => kr.RolId == adminRolId))
        {
            var digerAdminSayisi = await _context.KullaniciRols
                .CountAsync(kr => kr.RolId == adminRolId && kr.KullaniciId != id);
            if (digerAdminSayisi == 0)
            {
                TempData["Hata"] = "Sistemdeki son admin kullanıcısı silinemez! Önce başka bir kullanıcıya Admin rolü atayınız.";
                return RedirectToAction(nameof(KullaniciYonetimi), new { arama, durumFiltre, tipFiltre });
            }
        }

        var adSoyad = $"{kullanici.Ad} {kullanici.Soyad}";

        // İlişkili veriler (cascade olmayan)
        _context.KullaniciRols.RemoveRange(kullanici.KullaniciRols);
        _context.KullaniciYetkis.RemoveRange(kullanici.KullaniciYetkiKullanicis);

        // Ogrenci / OgretimUyesi
        if (kullanici.Ogrenci != null)
            _context.Ogrencis.Remove(kullanici.Ogrenci);
        if (kullanici.OgretimUyesi != null)
            _context.OgretimUyesis.Remove(kullanici.OgretimUyesi);

        _context.Kullanicis.Remove(kullanici);
        await _context.SaveChangesAsync();

        TempData["Basari"] = $"'{adSoyad}' kullanıcısı ve tüm ilişkili kayıtları başarıyla silindi.";
        return RedirectToAction(nameof(KullaniciYonetimi), new { arama, durumFiltre, tipFiltre });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ORGANİZASYON YÖNETİMİ
    // ─────────────────────────────────────────────────────────────────────────

    // GET: /Admin/OrganizasyonYonetimi
    [HttpGet]
    public async Task<IActionResult> OrganizasyonYonetimi(int? fakulteId)
    {
        // Tüm aktif + pasif organizasyonları çek
        var tumOrganizasyonlar = await _context.Organizasyons
            .OrderBy(o => o.Adi)
            .ToListAsync();

        // Fakülteler: UstOrganizasyonId null olanlar
        var fakulteler = tumOrganizasyonlar
            .Where(o => o.UstOrganizasyonId == null)
            .OrderBy(o => o.Adi)
            .ToList();

        // Seçili fakültenin bölümleri
        List<Organizasyon> bolumler = new();
        Organizasyon? secilenFakulte = null;
        if (fakulteId.HasValue && fakulteId.Value > 0)
        {
            secilenFakulte = fakulteler.FirstOrDefault(f => f.Id == fakulteId.Value);
            bolumler = tumOrganizasyonlar
                .Where(o => o.UstOrganizasyonId == fakulteId.Value)
                .OrderBy(o => o.Adi)
                .ToList();
        }

        ViewBag.Fakulteler = fakulteler;
        ViewBag.Bolumler = bolumler;
        ViewBag.SecilenFakulteId = fakulteId;
        ViewBag.SecilenFakulte = secilenFakulte;

        return View();
    }

    // POST: /Admin/FakulteEkle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FakulteEkle(string adi, string? kodu)
    {
        if (string.IsNullOrWhiteSpace(adi))
        {
            TempData["Hata"] = "Fakülte adı boş olamaz.";
            return RedirectToAction(nameof(OrganizasyonYonetimi));
        }

        adi = adi.Trim();
        if (await _context.Organizasyons.AnyAsync(o => o.Adi.ToLower() == adi.ToLower() && o.UstOrganizasyonId == null))
        {
            TempData["Hata"] = $"'{adi}' adında bir fakülte zaten mevcut.";
            return RedirectToAction(nameof(OrganizasyonYonetimi));
        }

        // Kodu uniqueness kontrolü
        if (!string.IsNullOrWhiteSpace(kodu))
        {
            var koduTemiz = kodu.Trim().ToUpper();
            if (await _context.Organizasyons.AnyAsync(o => o.Kodu == koduTemiz))
            {
                TempData["Hata"] = $"'{koduTemiz}' kodu başka bir birim tarafından kullanılmaktadır. Lütfen farklı bir kod girin.";
                return RedirectToAction(nameof(OrganizasyonYonetimi));
            }
        }

        var org = new Organizasyon
        {
            Adi = adi,
            Kodu = string.IsNullOrWhiteSpace(kodu) ? null : kodu.Trim().ToUpper(),
            Tipi = "Fakulte",
            UstOrganizasyonId = null,
            Durum = true
        };

        try
        {
            _context.Organizasyons.Add(org);
            await _context.SaveChangesAsync();
            TempData["Basari"] = $"'{org.Adi}' başarıyla eklendi.";
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            when (ex.InnerException?.Message.Contains("UQ_Organizasyon_Kodu") == true
               || ex.InnerException?.Message.Contains("unique") == true)
        {
            TempData["Hata"] = "Bu kod başka bir birim tarafından kullanılmaktadır. Lütfen farklı bir kod girin.";
        }
        catch (Exception)
        {
            TempData["Hata"] = "Kayıt sırasında beklenmedik bir hata oluştu. Lütfen tekrar deneyin.";
        }

        return RedirectToAction(nameof(OrganizasyonYonetimi));
    }

    // POST: /Admin/BolumEkle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BolumEkle(int fakulteId, string adi, string? kodu)
    {
        if (string.IsNullOrWhiteSpace(adi))
        {
            TempData["Hata"] = "Bölüm adı boş olamaz.";
            return RedirectToAction(nameof(OrganizasyonYonetimi), new { fakulteId });
        }

        var fakulte = await _context.Organizasyons.FindAsync(fakulteId);
        if (fakulte == null)
        {
            TempData["Hata"] = "Seçilen fakülte bulunamadı.";
            return RedirectToAction(nameof(OrganizasyonYonetimi));
        }

        adi = adi.Trim();
        if (await _context.Organizasyons.AnyAsync(o => o.Adi.ToLower() == adi.ToLower() && o.UstOrganizasyonId == fakulteId))
        {
            TempData["Hata"] = $"Bu fakültede '{adi}' adında bir bölüm zaten mevcut.";
            return RedirectToAction(nameof(OrganizasyonYonetimi), new { fakulteId });
        }

        // Kodu uniqueness kontrolü
        if (!string.IsNullOrWhiteSpace(kodu))
        {
            var koduTemiz = kodu.Trim().ToUpper();
            if (await _context.Organizasyons.AnyAsync(o => o.Kodu == koduTemiz))
            {
                TempData["Hata"] = $"'{koduTemiz}' kodu başka bir birim tarafından kullanılmaktadır. Lütfen farklı bir kod girin.";
                return RedirectToAction(nameof(OrganizasyonYonetimi), new { fakulteId });
            }
        }

        var org = new Organizasyon
        {
            Adi = adi,
            Kodu = string.IsNullOrWhiteSpace(kodu) ? null : kodu.Trim().ToUpper(),
            Tipi = "Bolum",
            UstOrganizasyonId = fakulteId,
            Durum = true
        };

        try
        {
            _context.Organizasyons.Add(org);
            await _context.SaveChangesAsync();
            TempData["Basari"] = $"'{org.Adi}' bölümü '{fakulte.Adi}' altına başarıyla eklendi.";
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            when (ex.InnerException?.Message.Contains("UQ_Organizasyon_Kodu") == true
               || ex.InnerException?.Message.Contains("unique") == true)
        {
            TempData["Hata"] = "Bu kod başka bir birim tarafından kullanılmaktadır. Lütfen farklı bir kod girin.";
        }
        catch (Exception)
        {
            TempData["Hata"] = "Kayıt sırasında beklenmedik bir hata oluştu. Lütfen tekrar deneyin.";
        }

        return RedirectToAction(nameof(OrganizasyonYonetimi), new { fakulteId });
    }

    // GET: /Admin/KoduKontrol  (AJAX)
    [HttpGet]
    public async Task<IActionResult> KoduKontrol(string kodu, int? mevcutId)
    {
        if (string.IsNullOrWhiteSpace(kodu))
            return Json(new { kullanildi = false });

        kodu = kodu.Trim().ToUpper();
        var sorgu = _context.Organizasyons.Where(o => o.Kodu == kodu);
        if (mevcutId.HasValue)
            sorgu = sorgu.Where(o => o.Id != mevcutId.Value);

        var kullanildi = await sorgu.AnyAsync();
        return Json(new { kullanildi });
    }

    // POST: /Admin/OrganizasyonSil
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OrganizasyonSil(int id, int? geriDonFakulteId)
    {
        var org = await _context.Organizasyons
            .Include(o => o.InverseUstOrganizasyon)
            .Include(o => o.Ogrencis)
            .Include(o => o.OgretimUyesis)
            .Include(o => o.Ders)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (org == null)
        {
            TempData["Hata"] = "Silinecek organizasyon bulunamadı.";
            return RedirectToAction(nameof(OrganizasyonYonetimi), new { fakulteId = geriDonFakulteId });
        }

        // Fakülte ise altında bölüm var mı?
        if (org.UstOrganizasyonId == null && org.InverseUstOrganizasyon.Any())
        {
            TempData["Hata"] = $"'{org.Adi}' fakültesine bağlı bölümler var. Önce bölümleri siliniz.";
            return RedirectToAction(nameof(OrganizasyonYonetimi), new { fakulteId = geriDonFakulteId });
        }

        // Bağlı öğrenci/öğretim üyesi/ders var mı?
        if (org.Ogrencis.Any())
        {
            TempData["Hata"] = $"'{org.Adi}' birimine kayıtlı {org.Ogrencis.Count} öğrenci bulunmaktadır. Önce öğrencileri başka birime aktarınız.";
            return RedirectToAction(nameof(OrganizasyonYonetimi), new { fakulteId = geriDonFakulteId });
        }
        if (org.OgretimUyesis.Any())
        {
            TempData["Hata"] = $"'{org.Adi}' birimine bağlı {org.OgretimUyesis.Count} öğretim üyesi bulunmaktadır. Önce öğretim üyelerini başka birime aktarınız.";
            return RedirectToAction(nameof(OrganizasyonYonetimi), new { fakulteId = geriDonFakulteId });
        }
        if (org.Ders.Any())
        {
            TempData["Hata"] = $"'{org.Adi}' birimine ait {org.Ders.Count} ders bulunmaktadır. Önce dersleri başka birime aktarınız.";
            return RedirectToAction(nameof(OrganizasyonYonetimi), new { fakulteId = geriDonFakulteId });
        }

        var adi = org.Adi;
        bool fakulteIdi = org.UstOrganizasyonId == null;

        _context.Organizasyons.Remove(org);
        await _context.SaveChangesAsync();

        TempData["Basari"] = $"'{adi}' başarıyla silindi.";

        // Fakülte silindiyse listeye dön, bölüm silindiyse aynı fakülte sayfasına dön
        if (fakulteIdi)
            return RedirectToAction(nameof(OrganizasyonYonetimi));
        else
            return RedirectToAction(nameof(OrganizasyonYonetimi), new { fakulteId = geriDonFakulteId });
    }

    // =========================================================================
    // 📅 DÖNEM YÖNETİMİ
    // =========================================================================

    // GET: /Admin/DonemYonetimi
    public async Task<IActionResult> DonemYonetimi()
    {
        var donemler = await _context.Donems
            .Include(d => d.AcilanDers)
            .OrderByDescending(d => d.AkademikYil)
            .ThenByDescending(d => d.BaslangicTarihi)
            .ToListAsync();

        return View(donemler);
    }

    // POST: /Admin/DonemEkle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DonemEkle(
        string akademikYil,
        string donem1,
        DateOnly baslangicTarihi,
        DateOnly bitisTarihi,
        DateTime dersKaydiBaslangic,
        DateTime dersKaydiBitis)
    {
        if (string.IsNullOrWhiteSpace(akademikYil) || string.IsNullOrWhiteSpace(donem1))
        {
            TempData["Hata"] = "Akademik Yıl ve Dönem alanları boş bırakılamaz.";
            return RedirectToAction(nameof(DonemYonetimi));
        }

        akademikYil = akademikYil.Trim();
        donem1 = donem1.Trim();
        if (donem1.Equals("Güz", StringComparison.OrdinalIgnoreCase))
            donem1 = "Guz";
        else if (donem1.Equals("Yaz Okulu", StringComparison.OrdinalIgnoreCase))
            donem1 = "Yaz";

        if (baslangicTarihi >= bitisTarihi)
        {
            TempData["Hata"] = "Dönem başlangıç tarihi bitiş tarihinden sonra veya eşit olamaz.";
            return RedirectToAction(nameof(DonemYonetimi));
        }

        if (dersKaydiBaslangic >= dersKaydiBitis)
        {
            TempData["Hata"] = "Ders kaydı başlangıç tarihi bitiş tarihinden sonra veya eşit olamaz.";
            return RedirectToAction(nameof(DonemYonetimi));
        }

        if (await _context.Donems.AnyAsync(d => d.AkademikYil == akademikYil && d.Donem1 == donem1))
        {
            string donemGoster = donem1 == "Guz" ? "Güz" : donem1;
            TempData["Hata"] = $"'{akademikYil} - {donemGoster}' akademik dönemi sistemde zaten mevcut.";
            return RedirectToAction(nameof(DonemYonetimi));
        }

        var donem = new Donem
        {
            AkademikYil = akademikYil,
            Donem1 = donem1,
            BaslangicTarihi = baslangicTarihi,
            BitisTarihi = bitisTarihi,
            DersKaydiBaslangic = dersKaydiBaslangic,
            DersKaydiBitis = dersKaydiBitis
        };

        try
        {
            _context.Donems.Add(donem);
            await _context.SaveChangesAsync();
            string donemGoster = donem.Donem1 == "Guz" ? "Güz" : donem.Donem1;
            TempData["Basari"] = $"'{donem.AkademikYil} - {donemGoster}' akademik dönemi başarıyla oluşturuldu.";
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            when (ex.InnerException?.Message.Contains("UQ_Donem_Yil_Donem") == true
               || ex.InnerException?.Message.Contains("unique") == true)
        {
            string donemGoster = donem1 == "Guz" ? "Güz" : donem1;
            TempData["Hata"] = $"'{akademikYil} - {donemGoster}' akademik dönemi sistemde zaten tanımlı.";
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            when (ex.InnerException?.Message.Contains("CK_Donem_Donem") == true)
        {
            TempData["Hata"] = $"Geçersiz dönem adı ('{donem1}'). Lütfen Güz, Bahar veya Yaz olarak seçin.";
        }
        catch (Exception)
        {
            TempData["Hata"] = "Dönem kaydı sırasında beklenmedik bir hata oluştu. Lütfen tekrar deneyin.";
        }

        return RedirectToAction(nameof(DonemYonetimi));
    }

    // POST: /Admin/DonemSil
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DonemSil(int id)
    {
        var donem = await _context.Donems
            .Include(d => d.AcilanDers)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (donem == null)
        {
            TempData["Hata"] = "Silinmek istenen dönem bulunamadı.";
            return RedirectToAction(nameof(DonemYonetimi));
        }

        string donemGoster = donem.Donem1 == "Guz" ? "Güz" : donem.Donem1;
        if (donem.AcilanDers.Any())
        {
            TempData["Hata"] = $"'{donem.AkademikYil} - {donemGoster}' dönemine ait {donem.AcilanDers.Count} açılan ders bulunmaktadır. Önce açılan dersleri silmeniz veya aktarmanız gerekmektedir.";
            return RedirectToAction(nameof(DonemYonetimi));
        }

        var ad = $"{donem.AkademikYil} - {donemGoster}";
        _context.Donems.Remove(donem);
        await _context.SaveChangesAsync();

        TempData["Basari"] = $"'{ad}' akademik dönemi başarıyla silindi.";
        return RedirectToAction(nameof(DonemYonetimi));
    }

    // GET: /Admin/DonemKontrol (AJAX)
    [HttpGet]
    public async Task<IActionResult> DonemKontrol(string akademikYil, string donem1, int? mevcutId)
    {
        if (string.IsNullOrWhiteSpace(akademikYil) || string.IsNullOrWhiteSpace(donem1))
            return Json(new { kullanildi = false });

        akademikYil = akademikYil.Trim();
        donem1 = donem1.Trim();
        if (donem1.Equals("Güz", StringComparison.OrdinalIgnoreCase))
            donem1 = "Guz";
        else if (donem1.Equals("Yaz Okulu", StringComparison.OrdinalIgnoreCase))
            donem1 = "Yaz";

        var sorgu = _context.Donems.Where(d => d.AkademikYil == akademikYil && d.Donem1 == donem1);
        if (mevcutId.HasValue)
            sorgu = sorgu.Where(d => d.Id != mevcutId.Value);

        var kullanildi = await sorgu.AnyAsync();
        return Json(new { kullanildi });
    }

    // =========================================================================
    // 📚 DERS KATALOĞU & HAVUZ YÖNETİMİ
    // =========================================================================

    // GET: /Admin/DersYonetimi
    public async Task<IActionResult> DersYonetimi(int? fakulteId, int? bolumId, string? arama)
    {
        // Fakülteleri (UstOrganizasyonId == null olanlar) ve Bölümleri getir
        var fakulteler = await _context.Organizasyons
            .Where(o => o.Durum && o.UstOrganizasyonId == null)
            .OrderBy(o => o.Adi)
            .ToListAsync();

        var bolumler = await _context.Organizasyons
            .Where(o => o.Durum && o.UstOrganizasyonId != null)
            .OrderBy(o => o.Adi)
            .ToListAsync();

        // Ön koşul ders seçimi için tüm aktif dersleri getir
        var tumDersler = await _context.Ders
            .Where(d => d.AktiflikDurumu)
            .OrderBy(d => d.DersKodu)
            .ToListAsync();

        var query = _context.Ders
            .Include(d => d.Organizasyon)
            .Include(d => d.AcilanDers)
            .Include(d => d.OnKosulDers)
            .AsQueryable();

        if (bolumId.HasValue && bolumId.Value > 0)
        {
            query = query.Where(d => d.OrganizasyonId == bolumId.Value);
        }
        else if (fakulteId.HasValue && fakulteId.Value > 0)
        {
            query = query.Where(d => d.Organizasyon.UstOrganizasyonId == fakulteId.Value);
        }

        if (!string.IsNullOrWhiteSpace(arama))
        {
            var a = arama.Trim().ToLower();
            query = query.Where(d => d.DersKodu.ToLower().Contains(a) || d.DersAdi.ToLower().Contains(a));
        }

        var dersler = await query
            .OrderBy(d => d.Organizasyon.Adi)
            .ThenBy(d => d.DersKodu)
            .ToListAsync();

        ViewBag.Fakulteler = fakulteler;
        ViewBag.Bolumler = bolumler;
        ViewBag.TumDersler = tumDersler;
        ViewBag.SeciliFakulteId = fakulteId;
        ViewBag.SeciliBolumId = bolumId;
        ViewBag.Arama = arama;

        return View(dersler);
    }

    // POST: /Admin/DersEkle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DersEkle(
        int organizasyonId,
        string dersKodu,
        string dersAdi,
        string kredi,
        string akts,
        int teorik,
        int uygulama,
        string dersTuru,
        int? onKosulDersId,
        int? geriDonFakulteId,
        int? geriDonBolumId)
    {
        if (organizasyonId <= 0 || string.IsNullOrWhiteSpace(dersKodu) || string.IsNullOrWhiteSpace(dersAdi))
        {
            TempData["Hata"] = "Bölüm, Ders Kodu ve Ders Adı alanları zorunludur.";
            return RedirectToAction(nameof(DersYonetimi), new { fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
        }

        dersKodu = dersKodu.Trim().ToUpper();
        dersAdi = dersAdi.Trim();
        dersTuru = (dersTuru ?? "Zorunlu").Trim();

        // ASCII normalizasyonu (DB CHECK kısıtları için)
        if (dersTuru.Equals("Seçmeli", StringComparison.OrdinalIgnoreCase))
            dersTuru = "Secmeli";
        else if (dersTuru.Equals("Zorunlu", StringComparison.OrdinalIgnoreCase))
            dersTuru = "Zorunlu";

        decimal krediVal = 0m;
        decimal aktsVal = 0m;
        if (!string.IsNullOrWhiteSpace(kredi))
        {
            var str = kredi.Trim().Replace(',', '.');
            decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out krediVal);
        }
        if (!string.IsNullOrWhiteSpace(akts))
        {
            var str = akts.Trim().Replace(',', '.');
            decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out aktsVal);
        }

        if (krediVal < 0 || aktsVal < 1 || teorik < 0 || uygulama < 0)
        {
            TempData["Hata"] = "Kredi, AKTS, Teorik ve Uygulama saatleri geçerli birer sayı olmalıdır.";
            return RedirectToAction(nameof(DersYonetimi), new { fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
        }

        if (await _context.Ders.AnyAsync(d => d.DersKodu == dersKodu))
        {
            TempData["Hata"] = $"'{dersKodu}' ders kodu sistemde başka bir ders tarafından kullanılmaktadır.";
            return RedirectToAction(nameof(DersYonetimi), new { fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
        }

        var der = new Der
        {
            OrganizasyonId = organizasyonId,
            DersKodu = dersKodu,
            DersAdi = dersAdi,
            Kredi = krediVal,
            Akts = aktsVal,
            Teorik = teorik,
            Uygulama = uygulama,
            DersTuru = dersTuru,
            AktiflikDurumu = true
        };

        if (onKosulDersId.HasValue && onKosulDersId.Value > 0)
        {
            var onKosul = await _context.Ders.FindAsync(onKosulDersId.Value);
            if (onKosul != null)
            {
                der.OnKosulDers.Add(onKosul);
            }
        }

        try
        {
            _context.Ders.Add(der);
            await _context.SaveChangesAsync();
            string turGoster = der.DersTuru == "Secmeli" ? "Seçmeli" : der.DersTuru;
            TempData["Basari"] = $"'{der.DersKodu} - {der.DersAdi}' ({turGoster}) başarıyla eklendi.";
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            when (ex.InnerException?.Message.Contains("UQ_Ders_DersKodu") == true
               || ex.InnerException?.Message.Contains("unique") == true)
        {
            TempData["Hata"] = $"'{dersKodu}' ders kodu sistemde zaten mevcut.";
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            when (ex.InnerException?.Message.Contains("CK_Ders_DersTuru") == true)
        {
            TempData["Hata"] = $"Geçersiz ders türü ('{dersTuru}'). Lütfen Zorunlu veya Seçmeli seçiniz.";
        }
        catch (Exception)
        {
            TempData["Hata"] = "Ders kaydı sırasında beklenmedik bir hata oluştu. Lütfen bilgileri kontrol edip tekrar deneyin.";
        }

        return RedirectToAction(nameof(DersYonetimi), new { fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
    }

    // POST: /Admin/DersDurumDegistir
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DersDurumDegistir(int id, int? geriDonFakulteId, int? geriDonBolumId)
    {
        var der = await _context.Ders.FindAsync(id);
        if (der == null)
        {
            TempData["Hata"] = "Ders bulunamadı.";
            return RedirectToAction(nameof(DersYonetimi), new { fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
        }

        der.AktiflikDurumu = !der.AktiflikDurumu;
        await _context.SaveChangesAsync();

        string durumMetin = der.AktiflikDurumu ? "Aktif" : "Pasif";
        TempData["Basari"] = $"'{der.DersKodu}' kodlu dersin durumu {durumMetin} yapıldı.";

        return RedirectToAction(nameof(DersYonetimi), new { fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
    }

    // POST: /Admin/DersSil
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DersSil(int id, int? geriDonFakulteId, int? geriDonBolumId)
    {
        var der = await _context.Ders
            .Include(d => d.AcilanDers)
            .Include(d => d.OnKosulDers)
            .Include(d => d.Ders)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (der == null)
        {
            TempData["Hata"] = "Silinmek istenen ders bulunamadı.";
            return RedirectToAction(nameof(DersYonetimi), new { fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
        }

        if (der.AcilanDers.Any())
        {
            TempData["Hata"] = $"'{der.DersKodu} - {der.DersAdi}' dersine ait {der.AcilanDers.Count} adet açılan dönem dersi bulunmaktadır. Bu nedenle ders silinemez; dilerseniz durumu 'Pasif' yapabilirsiniz.";
            return RedirectToAction(nameof(DersYonetimi), new { fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
        }

        if (der.Ders.Any())
        {
            var bagliKodlar = string.Join(", ", der.Ders.Select(x => x.DersKodu));
            TempData["Hata"] = $"'{der.DersKodu}' dersi başka derslerin ({bagliKodlar}) ön koşulu olduğu için silinemez. Önce ilgili derslerin ön koşul bağını kaldırınız.";
            return RedirectToAction(nameof(DersYonetimi), new { fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
        }

        der.OnKosulDers.Clear();

        var k = der.DersKodu;
        var a = der.DersAdi;
        _context.Ders.Remove(der);
        await _context.SaveChangesAsync();

        TempData["Basari"] = $"'{k} - {a}' sistemden silindi.";
        return RedirectToAction(nameof(DersYonetimi), new { fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
    }

    // GET: /Admin/DersKoduKontrol (AJAX)
    [HttpGet]
    public async Task<IActionResult> DersKoduKontrol(string dersKodu, int? mevcutId)
    {
        if (string.IsNullOrWhiteSpace(dersKodu))
            return Json(new { kullanildi = false });

        dersKodu = dersKodu.Trim().ToUpper();

        var sorgu = _context.Ders.Where(d => d.DersKodu == dersKodu);
        if (mevcutId.HasValue)
            sorgu = sorgu.Where(d => d.Id != mevcutId.Value);

        var kullanildi = await sorgu.AnyAsync();
        return Json(new { kullanildi });
    }

    // =========================================================================
    // AÇILAN DERS YÖNETİMİ (AcilanDer)
    // =========================================================================

    // GET: /Admin/AcilanDersYonetimi
    public async Task<IActionResult> AcilanDersYonetimi(int? donemId, int? fakulteId, int? bolumId, string? arama)
    {
        var donemler = await _context.Donems
            .OrderByDescending(d => d.BaslangicTarihi)
            .ToListAsync();

        // Eğer dönemi belirtmemişse varsayılan olarak en son dönemi seç
        if (!donemId.HasValue && donemler.Any())
        {
            donemId = donemler.First().Id;
        }

        var fakulteler = await _context.Organizasyons
            .Where(o => o.Durum && o.UstOrganizasyonId == null)
            .OrderBy(o => o.Adi)
            .ToListAsync();

        var bolumler = await _context.Organizasyons
            .Where(o => o.Durum && o.UstOrganizasyonId != null)
            .OrderBy(o => o.Adi)
            .ToListAsync();

        var tumDersler = await _context.Ders
            .Where(d => d.AktiflikDurumu)
            .OrderBy(d => d.DersKodu)
            .ToListAsync();

        var ogretimUyeleri = await _context.OgretimUyesis
            .Include(o => o.Kullanici)
            .Include(o => o.Organizasyon)
            .Where(o => o.Kullanici.AktiflikDurumu)
            .OrderBy(o => o.Kullanici.Ad)
            .ThenBy(o => o.Kullanici.Soyad)
            .ToListAsync();

        var query = _context.AcilanDers
            .Include(a => a.Ders)
                .ThenInclude(d => d.Organizasyon)
            .Include(a => a.Donem)
            .Include(a => a.OgretimUyesi)
                .ThenInclude(o => o.Kullanici)
            .Include(a => a.DersKaydis)
            .Include(a => a.DersProgramis)
            .AsQueryable();

        if (donemId.HasValue && donemId.Value > 0)
        {
            query = query.Where(a => a.DonemId == donemId.Value);
        }

        if (bolumId.HasValue && bolumId.Value > 0)
        {
            query = query.Where(a => a.Ders.OrganizasyonId == bolumId.Value);
        }
        else if (fakulteId.HasValue && fakulteId.Value > 0)
        {
            query = query.Where(a => a.Ders.Organizasyon.UstOrganizasyonId == fakulteId.Value);
        }

        if (!string.IsNullOrWhiteSpace(arama))
        {
            var a = arama.Trim().ToLower();
            query = query.Where(x => x.Ders.DersKodu.ToLower().Contains(a)
                                  || x.Ders.DersAdi.ToLower().Contains(a)
                                  || x.OgretimUyesi.Kullanici.Ad.ToLower().Contains(a)
                                  || x.OgretimUyesi.Kullanici.Soyad.ToLower().Contains(a)
                                  || x.SubeNo.ToLower().Contains(a));
        }

        var acilanDersler = await query
            .OrderByDescending(a => a.Donem.BaslangicTarihi)
            .ThenBy(a => a.Ders.DersKodu)
            .ThenBy(a => a.SubeNo)
            .ToListAsync();

        ViewBag.Donemler = donemler;
        ViewBag.Fakulteler = fakulteler;
        ViewBag.Bolumler = bolumler;
        ViewBag.TumDersler = tumDersler;
        ViewBag.OgretimUyeleri = ogretimUyeleri;
        ViewBag.SeciliDonemId = donemId;
        ViewBag.SeciliFakulteId = fakulteId;
        ViewBag.SeciliBolumId = bolumId;
        ViewBag.Arama = arama;

        return View(acilanDersler);
    }

    // POST: /Admin/AcilanDersEkle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcilanDersEkle(
        int dersId,
        int donemId,
        int ogretimUyesiId,
        string subeNo,
        int kontenjan,
        int? geriDonDonemId,
        int? geriDonFakulteId,
        int? geriDonBolumId)
    {
        if (dersId <= 0 || donemId <= 0 || ogretimUyesiId <= 0 || string.IsNullOrWhiteSpace(subeNo))
        {
            TempData["Hata"] = "Ders, Dönem, Öğretim Üyesi ve Şube Numarası alanları zorunludur.";
            return RedirectToAction(nameof(AcilanDersYonetimi), new { donemId = geriDonDonemId, fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
        }

        subeNo = subeNo.Trim().ToUpper();

        if (kontenjan < 1)
        {
            TempData["Hata"] = "Kontenjan en az 1 olmalıdır.";
            return RedirectToAction(nameof(AcilanDersYonetimi), new { donemId = geriDonDonemId, fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
        }

        if (await _context.AcilanDers.AnyAsync(a => a.DersId == dersId && a.DonemId == donemId && a.SubeNo == subeNo))
        {
            var d = await _context.Ders.FindAsync(dersId);
            string k = d?.DersKodu ?? "Seçilen ders";
            TempData["Hata"] = $"'{k}' dersi için bu dönemde '{subeNo}' şubesi zaten açılmış.";
            return RedirectToAction(nameof(AcilanDersYonetimi), new { donemId = geriDonDonemId, fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
        }

        var ad = new AcilanDer
        {
            DersId = dersId,
            DonemId = donemId,
            OgretimUyesiId = ogretimUyesiId,
            SubeNo = subeNo,
            Kontenjan = kontenjan,
            Durum = "Aktif"
        };

        try
        {
            _context.AcilanDers.Add(ad);
            await _context.SaveChangesAsync();

            var ders = await _context.Ders.FindAsync(dersId);
            TempData["Basari"] = $"'{ders?.DersKodu} - {ders?.DersAdi}' dersinin {subeNo} şubesi başarıyla açıldı.";
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            when (ex.InnerException?.Message.Contains("UQ_AcilanDers_Ders_Donem_Sube") == true
               || ex.InnerException?.Message.Contains("unique") == true)
        {
            TempData["Hata"] = $"Bu dersin {subeNo} şubesi seçili dönem için zaten açılmış.";
        }
        catch (Exception)
        {
            TempData["Hata"] = "Ders şubesi açılırken beklenmedik bir hata oluştu. Lütfen bilgileri kontrol edip tekrar deneyin.";
        }

        return RedirectToAction(nameof(AcilanDersYonetimi), new { donemId = geriDonDonemId, fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
    }

    // POST: /Admin/AcilanDersDurumDegistir
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcilanDersDurumDegistir(int id, int? geriDonDonemId, int? geriDonFakulteId, int? geriDonBolumId)
    {
        var ad = await _context.AcilanDers.Include(a => a.Ders).FirstOrDefaultAsync(a => a.Id == id);
        if (ad == null)
        {
            TempData["Hata"] = "Açılan ders şubesi bulunamadı.";
            return RedirectToAction(nameof(AcilanDersYonetimi), new { donemId = geriDonDonemId, fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
        }

        ad.Durum = (ad.Durum == "Aktif" || string.IsNullOrWhiteSpace(ad.Durum)) ? "Pasif" : "Aktif";
        await _context.SaveChangesAsync();

        TempData["Basari"] = $"'{ad.Ders?.DersKodu} - Şube: {ad.SubeNo}' durumu '{ad.Durum}' yapıldı.";
        return RedirectToAction(nameof(AcilanDersYonetimi), new { donemId = geriDonDonemId, fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
    }

    // POST: /Admin/AcilanDersSil
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcilanDersSil(int id, int? geriDonDonemId, int? geriDonFakulteId, int? geriDonBolumId)
    {
        var ad = await _context.AcilanDers
            .Include(a => a.Ders)
            .Include(a => a.DersKaydis)
            .Include(a => a.DersProgramis)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (ad == null)
        {
            TempData["Hata"] = "Silinmek istenen açılan ders bulunamadı.";
            return RedirectToAction(nameof(AcilanDersYonetimi), new { donemId = geriDonDonemId, fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
        }

        if (ad.DersKaydis.Any())
        {
            TempData["Hata"] = $"'{ad.Ders?.DersKodu} - Şube: {ad.SubeNo}' dersine kayıtlı {ad.DersKaydis.Count} öğrenci bulunmaktadır. Bu nedenle silinemez; dilerseniz durumu 'Pasif' yapabilirsiniz.";
            return RedirectToAction(nameof(AcilanDersYonetimi), new { donemId = geriDonDonemId, fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
        }

        if (ad.DersProgramis.Any())
        {
            TempData["Hata"] = $"'{ad.Ders?.DersKodu} - Şube: {ad.SubeNo}' dersine ait {ad.DersProgramis.Count} adet haftalık ders programı kaydı bulunmaktadır. Önce programdan kaldırınız.";
            return RedirectToAction(nameof(AcilanDersYonetimi), new { donemId = geriDonDonemId, fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
        }

        var k = ad.Ders?.DersKodu;
        var s = ad.SubeNo;
        _context.AcilanDers.Remove(ad);
        await _context.SaveChangesAsync();

        TempData["Basari"] = $"'{k} (Şube: {s})' sistemden silindi.";
        return RedirectToAction(nameof(AcilanDersYonetimi), new { donemId = geriDonDonemId, fakulteId = geriDonFakulteId, bolumId = geriDonBolumId });
    }

    // GET: /Admin/AcilanDersKontrol (AJAX)
    [HttpGet]
    public async Task<IActionResult> AcilanDersKontrol(int dersId, int donemId, string subeNo, int? mevcutId)
    {
        if (dersId <= 0 || donemId <= 0 || string.IsNullOrWhiteSpace(subeNo))
            return Json(new { kullanildi = false });

        subeNo = subeNo.Trim().ToUpper();

        var sorgu = _context.AcilanDers.Where(a => a.DersId == dersId && a.DonemId == donemId && a.SubeNo == subeNo);
        if (mevcutId.HasValue)
            sorgu = sorgu.Where(a => a.Id != mevcutId.Value);

        var kullanildi = await sorgu.AnyAsync();
        return Json(new { kullanildi });
    }
}

