
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

public class SalonsController : Controller
{
    private readonly ObsContext _context;

    public SalonsController(ObsContext context)
    {
        _context = context;
    }

    // GET: SALONS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Salons.ToListAsync());
    }

    // GET: SALONS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var salon = await _context.Salons
            .FirstOrDefaultAsync(m => m.Id == id);
        if (salon == null)
        {
            return NotFound();
        }

        return View(salon);
    }

    // GET: SALONS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: SALONS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,BinaId,SalonTipi,SalonAdi,Kapasite,Bina,DersProgramis,InverseBina,SinavProgramis")] Salon salon)
    {
        if (ModelState.IsValid)
        {
            _context.Add(salon);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(salon);
    }

    // GET: SALONS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var salon = await _context.Salons.FindAsync(id);
        if (salon == null)
        {
            return NotFound();
        }
        return View(salon);
    }

    // POST: SALONS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,BinaId,SalonTipi,SalonAdi,Kapasite,Bina,DersProgramis,InverseBina,SinavProgramis")] Salon salon)
    {
        if (id != salon.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(salon);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SalonExists(salon.Id))
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
        return View(salon);
    }

    // GET: SALONS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var salon = await _context.Salons
            .FirstOrDefaultAsync(m => m.Id == id);
        if (salon == null)
        {
            return NotFound();
        }

        return View(salon);
    }

    // POST: SALONS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var salon = await _context.Salons.FindAsync(id);
        if (salon != null)
        {
            _context.Salons.Remove(salon);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool SalonExists(int? id)
    {
        return _context.Salons.Any(e => e.Id == id);
    }
}
