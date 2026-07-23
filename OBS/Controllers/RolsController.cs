
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

public class RolsController : Controller
{
    private readonly ObsContext _context;

    public RolsController(ObsContext context)
    {
        _context = context;
    }

    // GET: ROLS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Rols.ToListAsync());
    }

    // GET: ROLS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var rol = await _context.Rols
            .FirstOrDefaultAsync(m => m.Id == id);
        if (rol == null)
        {
            return NotFound();
        }

        return View(rol);
    }

    // GET: ROLS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ROLS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,RolAdi,Aciklama,KullaniciRols,RolYetkis")] Rol rol)
    {
        if (ModelState.IsValid)
        {
            _context.Add(rol);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(rol);
    }

    // GET: ROLS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var rol = await _context.Rols.FindAsync(id);
        if (rol == null)
        {
            return NotFound();
        }
        return View(rol);
    }

    // POST: ROLS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,RolAdi,Aciklama,KullaniciRols,RolYetkis")] Rol rol)
    {
        if (id != rol.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(rol);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RolExists(rol.Id))
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
        return View(rol);
    }

    // GET: ROLS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var rol = await _context.Rols
            .FirstOrDefaultAsync(m => m.Id == id);
        if (rol == null)
        {
            return NotFound();
        }

        return View(rol);
    }

    // POST: ROLS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var rol = await _context.Rols.FindAsync(id);
        if (rol != null)
        {
            _context.Rols.Remove(rol);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool RolExists(int? id)
    {
        return _context.Rols.Any(e => e.Id == id);
    }
}
