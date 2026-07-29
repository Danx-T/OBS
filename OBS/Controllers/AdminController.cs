using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        var tipMesaj = model.KullaniciTipi == "Ogrenci" ? " (Öğrenci olarak)" :
                       model.KullaniciTipi == "OgretimUyesi" ? " (Öğretim Üyesi olarak)" : "";

        TempData["Basari"] = $"{kullanici.Ad} {kullanici.Soyad} adlı kullanıcı{tipMesaj} başarıyla oluşturuldu.";
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
}
