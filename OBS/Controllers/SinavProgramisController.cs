
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

public class SinavProgramisController : Controller
{
    private readonly ObsContext _context;

    public SinavProgramisController(ObsContext context)
    {
        _context = context;
    }

    // GET: SINAVPROGRAMIS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.SinavProgramis.ToListAsync());
    }

    // GET: SINAVPROGRAMIS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var sinavprogrami = await _context.SinavProgramis
            .FirstOrDefaultAsync(m => m.Id == id);
        if (sinavprogrami == null)
        {
            return NotFound();
        }

        return View(sinavprogrami);
    }

    // GET: SINAVPROGRAMIS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: SINAVPROGRAMIS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,AcilanDersId,SalonId,SinavTipi,Baslangic,Bitis,AcilanDers,Salon")] SinavProgrami sinavprogrami)
    {
        if (ModelState.IsValid)
        {
            _context.Add(sinavprogrami);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(sinavprogrami);
    }

    // GET: SINAVPROGRAMIS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var sinavprogrami = await _context.SinavProgramis.FindAsync(id);
        if (sinavprogrami == null)
        {
            return NotFound();
        }
        return View(sinavprogrami);
    }

    // POST: SINAVPROGRAMIS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,AcilanDersId,SalonId,SinavTipi,Baslangic,Bitis,AcilanDers,Salon")] SinavProgrami sinavprogrami)
    {
        if (id != sinavprogrami.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(sinavprogrami);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SinavProgramiExists(sinavprogrami.Id))
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
        return View(sinavprogrami);
    }

    // GET: SINAVPROGRAMIS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var sinavprogrami = await _context.SinavProgramis
            .FirstOrDefaultAsync(m => m.Id == id);
        if (sinavprogrami == null)
        {
            return NotFound();
        }

        return View(sinavprogrami);
    }

    // POST: SINAVPROGRAMIS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var sinavprogrami = await _context.SinavProgramis.FindAsync(id);
        if (sinavprogrami != null)
        {
            _context.SinavProgramis.Remove(sinavprogrami);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool SinavProgramiExists(int? id)
    {
        return _context.SinavProgramis.Any(e => e.Id == id);
    }
}
