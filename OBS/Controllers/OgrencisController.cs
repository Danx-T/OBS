
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

public class OgrencisController : Controller
{
    private readonly ObsContext _context;

    public OgrencisController(ObsContext context)
    {
        _context = context;
    }

    // GET: OGRENCIS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Ogrencis.ToListAsync());
    }

    // GET: OGRENCIS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ogrenci = await _context.Ogrencis
            .FirstOrDefaultAsync(m => m.Id == id);
        if (ogrenci == null)
        {
            return NotFound();
        }

        return View(ogrenci);
    }

    // GET: OGRENCIS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: OGRENCIS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,KullaniciId,Cinsiyet,OgrenciNo,DanismanId,OrganizasyonId,GirisTarihi,OgrenciTipi,Durum,MezuniyetTarihi,Sinif,Danisman,DersKaydis,Kullanici,Organizasyon")] Ogrenci ogrenci)
    {
        if (ModelState.IsValid)
        {
            _context.Add(ogrenci);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(ogrenci);
    }

    // GET: OGRENCIS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ogrenci = await _context.Ogrencis.FindAsync(id);
        if (ogrenci == null)
        {
            return NotFound();
        }
        return View(ogrenci);
    }

    // POST: OGRENCIS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,KullaniciId,Cinsiyet,OgrenciNo,DanismanId,OrganizasyonId,GirisTarihi,OgrenciTipi,Durum,MezuniyetTarihi,Sinif,Danisman,DersKaydis,Kullanici,Organizasyon")] Ogrenci ogrenci)
    {
        if (id != ogrenci.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(ogrenci);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OgrenciExists(ogrenci.Id))
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
        return View(ogrenci);
    }

    // GET: OGRENCIS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ogrenci = await _context.Ogrencis
            .FirstOrDefaultAsync(m => m.Id == id);
        if (ogrenci == null)
        {
            return NotFound();
        }

        return View(ogrenci);
    }

    // POST: OGRENCIS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var ogrenci = await _context.Ogrencis.FindAsync(id);
        if (ogrenci != null)
        {
            _context.Ogrencis.Remove(ogrenci);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool OgrenciExists(int? id)
    {
        return _context.Ogrencis.Any(e => e.Id == id);
    }
}
