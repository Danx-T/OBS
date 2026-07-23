using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OBS.Models;

namespace OBS.Controllers
{
    public class RolYetkisController : Controller
    {
        private readonly ObsContext _context;

        public RolYetkisController(ObsContext context)
        {
            _context = context;
        }

        // GET: RolYetkis
        public async Task<IActionResult> Index()
        {
            var obsContext = _context.RolYetkis.Include(r => r.Rol).Include(r => r.Yetki);
            return View(await obsContext.ToListAsync());
        }

        // GET: RolYetkis/Details/5/2
        public async Task<IActionResult> Details(int? rolId, int? yetkiId)
        {
            if (rolId == null || yetkiId == null)
            {
                return NotFound();
            }

            var rolYetki = await _context.RolYetkis
                .Include(r => r.Rol)
                .Include(r => r.Yetki)
                .FirstOrDefaultAsync(m => m.RolId == rolId && m.YetkiId == yetkiId);
            if (rolYetki == null)
            {
                return NotFound();
            }

            return View(rolYetki);
        }

        // GET: RolYetkis/Create
        public IActionResult Create()
        {
            ViewData["RolId"] = new SelectList(_context.Rols, "Id", "Id");
            ViewData["YetkiId"] = new SelectList(_context.Yetkis, "Id", "Id");
            return View();
        }

        // POST: RolYetkis/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RolId,YetkiId,BaslangicTarihi,BitisTarihi")] RolYetki rolYetki)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rolYetki);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RolId"] = new SelectList(_context.Rols, "Id", "Id", rolYetki.RolId);
            ViewData["YetkiId"] = new SelectList(_context.Yetkis, "Id", "Id", rolYetki.YetkiId);
            return View(rolYetki);
        }

        // GET: RolYetkis/Edit/5/2
        public async Task<IActionResult> Edit(int? rolId, int? yetkiId)
        {
            if (rolId == null || yetkiId == null)
            {
                return NotFound();
            }

            var rolYetki = await _context.RolYetkis.FindAsync(rolId, yetkiId);
            if (rolYetki == null)
            {
                return NotFound();
            }
            ViewData["RolId"] = new SelectList(_context.Rols, "Id", "Id", rolYetki.RolId);
            ViewData["YetkiId"] = new SelectList(_context.Yetkis, "Id", "Id", rolYetki.YetkiId);
            return View(rolYetki);
        }

        // POST: RolYetkis/Edit/5/2
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int rolId, int yetkiId, [Bind("RolId,YetkiId,BaslangicTarihi,BitisTarihi")] RolYetki rolYetki)
        {
            if (rolId != rolYetki.RolId || yetkiId != rolYetki.YetkiId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rolYetki);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RolYetkiExists(rolYetki.RolId, rolYetki.YetkiId))
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
            ViewData["RolId"] = new SelectList(_context.Rols, "Id", "Id", rolYetki.RolId);
            ViewData["YetkiId"] = new SelectList(_context.Yetkis, "Id", "Id", rolYetki.YetkiId);
            return View(rolYetki);
        }

        // GET: RolYetkis/Delete/5/2
        public async Task<IActionResult> Delete(int? rolId, int? yetkiId)
        {
            if (rolId == null || yetkiId == null)
            {
                return NotFound();
            }

            var rolYetki = await _context.RolYetkis
                .Include(r => r.Rol)
                .Include(r => r.Yetki)
                .FirstOrDefaultAsync(m => m.RolId == rolId && m.YetkiId == yetkiId);
            if (rolYetki == null)
            {
                return NotFound();
            }

            return View(rolYetki);
        }

        // POST: RolYetkis/Delete/5/2
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int rolId, int yetkiId)
        {
            var rolYetki = await _context.RolYetkis.FindAsync(rolId, yetkiId);
            if (rolYetki != null)
            {
                _context.RolYetkis.Remove(rolYetki);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RolYetkiExists(int rolId, int yetkiId)
        {
            return _context.RolYetkis.Any(e => e.RolId == rolId && e.YetkiId == yetkiId);
        }
    }
}