
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

public class NotlarsController : Controller
{
    private readonly ObsContext _context;

    public NotlarsController(ObsContext context)
    {
        _context = context;
    }

    // GET: NOTLARS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Notlars.ToListAsync());
    }

    // GET: NOTLARS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var notlar = await _context.Notlars
            .FirstOrDefaultAsync(m => m.Id == id);
        if (notlar == null)
        {
            return NotFound();
        }

        return View(notlar);
    }

    // GET: NOTLARS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: NOTLARS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,DersKaydiId,OlcmeTuru,Puan,DersKaydi")] Notlar notlar)
    {
        if (ModelState.IsValid)
        {
            _context.Add(notlar);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(notlar);
    }

    // GET: NOTLARS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var notlar = await _context.Notlars.FindAsync(id);
        if (notlar == null)
        {
            return NotFound();
        }
        return View(notlar);
    }

    // POST: NOTLARS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,DersKaydiId,OlcmeTuru,Puan,DersKaydi")] Notlar notlar)
    {
        if (id != notlar.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(notlar);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotlarExists(notlar.Id))
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
        return View(notlar);
    }

    // GET: NOTLARS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var notlar = await _context.Notlars
            .FirstOrDefaultAsync(m => m.Id == id);
        if (notlar == null)
        {
            return NotFound();
        }

        return View(notlar);
    }

    // POST: NOTLARS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var notlar = await _context.Notlars.FindAsync(id);
        if (notlar != null)
        {
            _context.Notlars.Remove(notlar);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool NotlarExists(int? id)
    {
        return _context.Notlars.Any(e => e.Id == id);
    }
}
