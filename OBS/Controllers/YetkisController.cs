
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

public class YetkisController : Controller
{
    private readonly ObsContext _context;

    public YetkisController(ObsContext context)
    {
        _context = context;
    }

    // GET: YETKIS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Yetkis.ToListAsync());
    }

    // GET: YETKIS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var yetki = await _context.Yetkis
            .FirstOrDefaultAsync(m => m.Id == id);
        if (yetki == null)
        {
            return NotFound();
        }

        return View(yetki);
    }

    // GET: YETKIS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: YETKIS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,YetkiKodu,Aciklama,KullaniciYetkis,RolYetkis")] Yetki yetki)
    {
        if (ModelState.IsValid)
        {
            _context.Add(yetki);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(yetki);
    }

    // GET: YETKIS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var yetki = await _context.Yetkis.FindAsync(id);
        if (yetki == null)
        {
            return NotFound();
        }
        return View(yetki);
    }

    // POST: YETKIS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,YetkiKodu,Aciklama,KullaniciYetkis,RolYetkis")] Yetki yetki)
    {
        if (id != yetki.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(yetki);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!YetkiExists(yetki.Id))
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
        return View(yetki);
    }

    // GET: YETKIS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var yetki = await _context.Yetkis
            .FirstOrDefaultAsync(m => m.Id == id);
        if (yetki == null)
        {
            return NotFound();
        }

        return View(yetki);
    }

    // POST: YETKIS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var yetki = await _context.Yetkis.FindAsync(id);
        if (yetki != null)
        {
            _context.Yetkis.Remove(yetki);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool YetkiExists(int? id)
    {
        return _context.Yetkis.Any(e => e.Id == id);
    }
}
