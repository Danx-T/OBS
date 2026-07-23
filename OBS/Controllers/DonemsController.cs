
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

public class DonemsController : Controller
{
    private readonly ObsContext _context;

    public DonemsController(ObsContext context)
    {
        _context = context;
    }

    // GET: DONEMS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Donems.ToListAsync());
    }

    // GET: DONEMS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var donem = await _context.Donems
            .FirstOrDefaultAsync(m => m.Id == id);
        if (donem == null)
        {
            return NotFound();
        }

        return View(donem);
    }

    // GET: DONEMS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: DONEMS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,AkademikYil,Donem1,BaslangicTarihi,BitisTarihi,DersKaydiBaslangic,DersKaydiBitis,AcilanDers")] Donem donem)
    {
        if (ModelState.IsValid)
        {
            _context.Add(donem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(donem);
    }

    // GET: DONEMS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var donem = await _context.Donems.FindAsync(id);
        if (donem == null)
        {
            return NotFound();
        }
        return View(donem);
    }

    // POST: DONEMS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,AkademikYil,Donem1,BaslangicTarihi,BitisTarihi,DersKaydiBaslangic,DersKaydiBitis,AcilanDers")] Donem donem)
    {
        if (id != donem.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(donem);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DonemExists(donem.Id))
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
        return View(donem);
    }

    // GET: DONEMS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var donem = await _context.Donems
            .FirstOrDefaultAsync(m => m.Id == id);
        if (donem == null)
        {
            return NotFound();
        }

        return View(donem);
    }

    // POST: DONEMS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var donem = await _context.Donems.FindAsync(id);
        if (donem != null)
        {
            _context.Donems.Remove(donem);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool DonemExists(int? id)
    {
        return _context.Donems.Any(e => e.Id == id);
    }
}
