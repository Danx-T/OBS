
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

public class KullanicisController : Controller
{
    private readonly ObsContext _context;

    public KullanicisController(ObsContext context)
    {
        _context = context;
    }

    // GET: KULLANICIS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Kullanicis.ToListAsync());
    }

    // GET: KULLANICIS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var kullanici = await _context.Kullanicis
            .FirstOrDefaultAsync(m => m.Id == id);
        if (kullanici == null)
        {
            return NotFound();
        }

        return View(kullanici);
    }

    // GET: KULLANICIS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: KULLANICIS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Ad,Soyad,Eposta,Telefon,SifreHash,IkiFaktorluDogrulama,AktiflikDurumu,OlusturmaTarihi,SonGuncellenmeTarihi,DenetimKaydis,KullaniciRols,KullaniciYetkiIslemYapanKullanicis,KullaniciYetkiKullanicis,Ogrenci,OgretimUyesi")] Kullanici kullanici)
    {
        if (ModelState.IsValid)
        {
            _context.Add(kullanici);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(kullanici);
    }

    // GET: KULLANICIS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var kullanici = await _context.Kullanicis.FindAsync(id);
        if (kullanici == null)
        {
            return NotFound();
        }
        return View(kullanici);
    }

    // POST: KULLANICIS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Ad,Soyad,Eposta,Telefon,SifreHash,IkiFaktorluDogrulama,AktiflikDurumu,OlusturmaTarihi,SonGuncellenmeTarihi,DenetimKaydis,KullaniciRols,KullaniciYetkiIslemYapanKullanicis,KullaniciYetkiKullanicis,Ogrenci,OgretimUyesi")] Kullanici kullanici)
    {
        if (id != kullanici.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(kullanici);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KullaniciExists(kullanici.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(kullanici);
    }

    // GET: KULLANICIS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var kullanici = await _context.Kullanicis
            .FirstOrDefaultAsync(m => m.Id == id);
        if (kullanici == null)
        {
            return NotFound();
        }

        return View(kullanici);
    }

    // POST: KULLANICIS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var kullanici = await _context.Kullanicis.FindAsync(id);
        if (kullanici != null)
        {
            _context.Kullanicis.Remove(kullanici);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool KullaniciExists(int? id)
    {
        return _context.Kullanicis.Any(e => e.Id == id);
    }
}
