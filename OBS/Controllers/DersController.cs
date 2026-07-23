
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

public class DersController : Controller
{
    private readonly ObsContext _context;

    public DersController(ObsContext context)
    {
        _context = context;
    }

    // GET: DERS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Ders.ToListAsync());
    }

    // GET: DERS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var der = await _context.Ders
            .FirstOrDefaultAsync(m => m.Id == id);
        if (der == null)
        {
            return NotFound();
        }

        return View(der);
    }

    // GET: DERS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: DERS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,OrganizasyonId,DersKodu,DersAdi,Kredi,Akts,Teorik,Uygulama,DersTuru,AktiflikDurumu,AcilanDers,Organizasyon,Ders,OnKosulDers")] Der der)
    {
        if (ModelState.IsValid)
        {
            _context.Add(der);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(der);
    }

    // GET: DERS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var der = await _context.Ders.FindAsync(id);
        if (der == null)
        {
            return NotFound();
        }
        return View(der);
    }

    // POST: DERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,OrganizasyonId,DersKodu,DersAdi,Kredi,Akts,Teorik,Uygulama,DersTuru,AktiflikDurumu,AcilanDers,Organizasyon,Ders,OnKosulDers")] Der der)
    {
        if (id != der.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(der);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DerExists(der.Id))
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
        return View(der);
    }

    // GET: DERS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var der = await _context.Ders
            .FirstOrDefaultAsync(m => m.Id == id);
        if (der == null)
        {
            return NotFound();
        }

        return View(der);
    }

    // POST: DERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var der = await _context.Ders.FindAsync(id);
        if (der != null)
        {
            _context.Ders.Remove(der);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool DerExists(int? id)
    {
        return _context.Ders.Any(e => e.Id == id);
    }
}
