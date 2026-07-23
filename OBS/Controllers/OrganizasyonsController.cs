
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

public class OrganizasyonsController : Controller
{
    private readonly ObsContext _context;

    public OrganizasyonsController(ObsContext context)
    {
        _context = context;
    }

    // GET: ORGANIZASYONS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Organizasyons.ToListAsync());
    }

    // GET: ORGANIZASYONS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var organizasyon = await _context.Organizasyons
            .FirstOrDefaultAsync(m => m.Id == id);
        if (organizasyon == null)
        {
            return NotFound();
        }

        return View(organizasyon);
    }

    // GET: ORGANIZASYONS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ORGANIZASYONS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,UstOrganizasyonId,Tipi,Adi,Kodu,Durum,Ders,InverseUstOrganizasyon,Ogrencis,OgretimUyesis,UstOrganizasyon")] Organizasyon organizasyon)
    {
        if (ModelState.IsValid)
        {
            _context.Add(organizasyon);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(organizasyon);
    }

    // GET: ORGANIZASYONS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var organizasyon = await _context.Organizasyons.FindAsync(id);
        if (organizasyon == null)
        {
            return NotFound();
        }
        return View(organizasyon);
    }

    // POST: ORGANIZASYONS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,UstOrganizasyonId,Tipi,Adi,Kodu,Durum,Ders,InverseUstOrganizasyon,Ogrencis,OgretimUyesis,UstOrganizasyon")] Organizasyon organizasyon)
    {
        if (id != organizasyon.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(organizasyon);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrganizasyonExists(organizasyon.Id))
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
        return View(organizasyon);
    }

    // GET: ORGANIZASYONS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var organizasyon = await _context.Organizasyons
            .FirstOrDefaultAsync(m => m.Id == id);
        if (organizasyon == null)
        {
            return NotFound();
        }

        return View(organizasyon);
    }

    // POST: ORGANIZASYONS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var organizasyon = await _context.Organizasyons.FindAsync(id);
        if (organizasyon != null)
        {
            _context.Organizasyons.Remove(organizasyon);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool OrganizasyonExists(int? id)
    {
        return _context.Organizasyons.Any(e => e.Id == id);
    }
}
