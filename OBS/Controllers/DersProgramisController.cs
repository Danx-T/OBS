
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

public class DersProgramisController : Controller
{
    private readonly ObsContext _context;

    public DersProgramisController(ObsContext context)
    {
        _context = context;
    }

    // GET: DERSPROGRAMIS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.DersProgramis.ToListAsync());
    }

    // GET: DERSPROGRAMIS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var dersprogrami = await _context.DersProgramis
            .FirstOrDefaultAsync(m => m.Id == id);
        if (dersprogrami == null)
        {
            return NotFound();
        }

        return View(dersprogrami);
    }

    // GET: DERSPROGRAMIS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: DERSPROGRAMIS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,AcilanDersId,SalonId,Gun,BaslangicSaati,BitisSaati,AcilanDers,Salon")] DersProgrami dersprogrami)
    {
        if (ModelState.IsValid)
        {
            _context.Add(dersprogrami);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(dersprogrami);
    }

    // GET: DERSPROGRAMIS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var dersprogrami = await _context.DersProgramis.FindAsync(id);
        if (dersprogrami == null)
        {
            return NotFound();
        }
        return View(dersprogrami);
    }

    // POST: DERSPROGRAMIS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,AcilanDersId,SalonId,Gun,BaslangicSaati,BitisSaati,AcilanDers,Salon")] DersProgrami dersprogrami)
    {
        if (id != dersprogrami.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(dersprogrami);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DersProgramiExists(dersprogrami.Id))
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
        return View(dersprogrami);
    }

    // GET: DERSPROGRAMIS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var dersprogrami = await _context.DersProgramis
            .FirstOrDefaultAsync(m => m.Id == id);
        if (dersprogrami == null)
        {
            return NotFound();
        }

        return View(dersprogrami);
    }

    // POST: DERSPROGRAMIS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var dersprogrami = await _context.DersProgramis.FindAsync(id);
        if (dersprogrami != null)
        {
            _context.DersProgramis.Remove(dersprogrami);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool DersProgramiExists(int? id)
    {
        return _context.DersProgramis.Any(e => e.Id == id);
    }
}
