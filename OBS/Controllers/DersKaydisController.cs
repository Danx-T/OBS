
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

public class DersKaydisController : Controller
{
    private readonly ObsContext _context;

    public DersKaydisController(ObsContext context)
    {
        _context = context;
    }

    // GET: DERSKAYDIS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.DersKaydis.ToListAsync());
    }

    // GET: DERSKAYDIS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var derskaydi = await _context.DersKaydis
            .FirstOrDefaultAsync(m => m.Id == id);
        if (derskaydi == null)
        {
            return NotFound();
        }

        return View(derskaydi);
    }

    // GET: DERSKAYDIS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: DERSKAYDIS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,OgrenciId,AcilanDersId,KayitDurumu,KayitTarihi,OnayTarihi,AcilanDers,Notlars,Ogrenci")] DersKaydi derskaydi)
    {
        if (ModelState.IsValid)
        {
            _context.Add(derskaydi);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(derskaydi);
    }

    // GET: DERSKAYDIS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var derskaydi = await _context.DersKaydis.FindAsync(id);
        if (derskaydi == null)
        {
            return NotFound();
        }
        return View(derskaydi);
    }

    // POST: DERSKAYDIS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,OgrenciId,AcilanDersId,KayitDurumu,KayitTarihi,OnayTarihi,AcilanDers,Notlars,Ogrenci")] DersKaydi derskaydi)
    {
        if (id != derskaydi.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(derskaydi);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DersKaydiExists(derskaydi.Id))
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
        return View(derskaydi);
    }

    // GET: DERSKAYDIS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var derskaydi = await _context.DersKaydis
            .FirstOrDefaultAsync(m => m.Id == id);
        if (derskaydi == null)
        {
            return NotFound();
        }

        return View(derskaydi);
    }

    // POST: DERSKAYDIS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var derskaydi = await _context.DersKaydis.FindAsync(id);
        if (derskaydi != null)
        {
            _context.DersKaydis.Remove(derskaydi);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool DersKaydiExists(int? id)
    {
        return _context.DersKaydis.Any(e => e.Id == id);
    }
}
