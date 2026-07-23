
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

public class OgretimUyesisController : Controller
{
    private readonly ObsContext _context;

    public OgretimUyesisController(ObsContext context)
    {
        _context = context;
    }

    // GET: OGRETIMUYESIS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.OgretimUyesis.ToListAsync());
    }

    // GET: OGRETIMUYESIS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ogretimuyesi = await _context.OgretimUyesis
            .FirstOrDefaultAsync(m => m.Id == id);
        if (ogretimuyesi == null)
        {
            return NotFound();
        }

        return View(ogretimuyesi);
    }

    // GET: OGRETIMUYESIS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: OGRETIMUYESIS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,KullaniciId,Cinsiyet,Unvan,OrganizasyonId,KadroTipi,GorevBaslangic,GorevBitis,AcilanDers,Kullanici,Ogrencis,Organizasyon")] OgretimUyesi ogretimuyesi)
    {
        if (ModelState.IsValid)
        {
            _context.Add(ogretimuyesi);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(ogretimuyesi);
    }

    // GET: OGRETIMUYESIS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ogretimuyesi = await _context.OgretimUyesis.FindAsync(id);
        if (ogretimuyesi == null)
        {
            return NotFound();
        }
        return View(ogretimuyesi);
    }

    // POST: OGRETIMUYESIS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,KullaniciId,Cinsiyet,Unvan,OrganizasyonId,KadroTipi,GorevBaslangic,GorevBitis,AcilanDers,Kullanici,Ogrencis,Organizasyon")] OgretimUyesi ogretimuyesi)
    {
        if (id != ogretimuyesi.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(ogretimuyesi);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OgretimUyesiExists(ogretimuyesi.Id))
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
        return View(ogretimuyesi);
    }

    // GET: OGRETIMUYESIS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ogretimuyesi = await _context.OgretimUyesis
            .FirstOrDefaultAsync(m => m.Id == id);
        if (ogretimuyesi == null)
        {
            return NotFound();
        }

        return View(ogretimuyesi);
    }

    // POST: OGRETIMUYESIS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var ogretimuyesi = await _context.OgretimUyesis.FindAsync(id);
        if (ogretimuyesi != null)
        {
            _context.OgretimUyesis.Remove(ogretimuyesi);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool OgretimUyesiExists(int? id)
    {
        return _context.OgretimUyesis.Any(e => e.Id == id);
    }
}
