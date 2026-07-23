
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

public class AcilanDersController : Controller
{
    private readonly ObsContext _context;

    public AcilanDersController(ObsContext context)
    {
        _context = context;
    }

    // GET: ACILANDERS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.AcilanDers.ToListAsync());
    }

    // GET: ACILANDERS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var acilander = await _context.AcilanDers
            .FirstOrDefaultAsync(m => m.Id == id);
        if (acilander == null)
        {
            return NotFound();
        }

        return View(acilander);
    }

    // GET: ACILANDERS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ACILANDERS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,DersId,OgretimUyesiId,DonemId,SubeNo,Kontenjan,Durum,Ders,DersKaydis,DersProgramis,Donem,OgretimUyesi,SinavProgramis")] AcilanDer acilander)
    {
        if (ModelState.IsValid)
        {
            _context.Add(acilander);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(acilander);
    }

    // GET: ACILANDERS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var acilander = await _context.AcilanDers.FindAsync(id);
        if (acilander == null)
        {
            return NotFound();
        }
        return View(acilander);
    }

    // POST: ACILANDERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,DersId,OgretimUyesiId,DonemId,SubeNo,Kontenjan,Durum,Ders,DersKaydis,DersProgramis,Donem,OgretimUyesi,SinavProgramis")] AcilanDer acilander)
    {
        if (id != acilander.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(acilander);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AcilanDerExists(acilander.Id))
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
        return View(acilander);
    }

    // GET: ACILANDERS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var acilander = await _context.AcilanDers
            .FirstOrDefaultAsync(m => m.Id == id);
        if (acilander == null)
        {
            return NotFound();
        }

        return View(acilander);
    }

    // POST: ACILANDERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var acilander = await _context.AcilanDers.FindAsync(id);
        if (acilander != null)
        {
            _context.AcilanDers.Remove(acilander);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool AcilanDerExists(int? id)
    {
        return _context.AcilanDers.Any(e => e.Id == id);
    }
}
