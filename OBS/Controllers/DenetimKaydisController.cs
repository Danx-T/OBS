
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

public class DenetimKaydisController : Controller
{
    private readonly ObsContext _context;

    public DenetimKaydisController(ObsContext context)
    {
        _context = context;
    }

    // GET: DENETIMKAYDIS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.DenetimKaydis.ToListAsync());
    }

    // GET: DENETIMKAYDIS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var denetimkaydi = await _context.DenetimKaydis
            .FirstOrDefaultAsync(m => m.Id == id);
        if (denetimkaydi == null)
        {
            return NotFound();
        }

        return View(denetimkaydi);
    }

    // GET: DENETIMKAYDIS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: DENETIMKAYDIS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,KullaniciId,IslemTuru,EtkilenenTablo,EtkilenenKayitId,EtkilenenSutun,EskiDeger,YeniDeger,IslemZamani,IpAdresi,Kullanici")] DenetimKaydi denetimkaydi)
    {
        if (ModelState.IsValid)
        {
            _context.Add(denetimkaydi);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(denetimkaydi);
    }

    // GET: DENETIMKAYDIS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var denetimkaydi = await _context.DenetimKaydis.FindAsync(id);
        if (denetimkaydi == null)
        {
            return NotFound();
        }
        return View(denetimkaydi);
    }

    // POST: DENETIMKAYDIS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,KullaniciId,IslemTuru,EtkilenenTablo,EtkilenenKayitId,EtkilenenSutun,EskiDeger,YeniDeger,IslemZamani,IpAdresi,Kullanici")] DenetimKaydi denetimkaydi)
    {
        if (id != denetimkaydi.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(denetimkaydi);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DenetimKaydiExists(denetimkaydi.Id))
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
        return View(denetimkaydi);
    }

    // GET: DENETIMKAYDIS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var denetimkaydi = await _context.DenetimKaydis
            .FirstOrDefaultAsync(m => m.Id == id);
        if (denetimkaydi == null)
        {
            return NotFound();
        }

        return View(denetimkaydi);
    }

    // POST: DENETIMKAYDIS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var denetimkaydi = await _context.DenetimKaydis.FindAsync(id);
        if (denetimkaydi != null)
        {
            _context.DenetimKaydis.Remove(denetimkaydi);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool DenetimKaydiExists(int? id)
    {
        return _context.DenetimKaydis.Any(e => e.Id == id);
    }
}
