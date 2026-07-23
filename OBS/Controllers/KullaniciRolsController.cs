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
    public class KullaniciRolsController : Controller
    {
        private readonly ObsContext _context;

        public KullaniciRolsController(ObsContext context)
        {
            _context = context;
        }

        // GET: KullaniciRols
        public async Task<IActionResult> Index()
        {
            var obsContext = _context.KullaniciRols.Include(k => k.Kullanici).Include(k => k.Rol);
            return View(await obsContext.ToListAsync());
        }

        // GET: KullaniciRols/Details/5/2
        public async Task<IActionResult> Details(int? kullaniciId, int? rolId)
        {
            if (kullaniciId == null || rolId == null)
            {
                return NotFound();
            }

            var kullaniciRol = await _context.KullaniciRols
                .Include(k => k.Kullanici)
                .Include(k => k.Rol)
                .FirstOrDefaultAsync(m => m.KullaniciId == kullaniciId && m.RolId == rolId);
            if (kullaniciRol == null)
            {
                return NotFound();
            }

            return View(kullaniciRol);
        }

        // GET: KullaniciRols/Create
        public IActionResult Create()
        {
            ViewData["KullaniciId"] = new SelectList(_context.Kullanicis, "Id", "Id");
            ViewData["RolId"] = new SelectList(_context.Rols, "Id", "Id");
            return View();
        }

        // POST: KullaniciRols/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("KullaniciId,RolId,AktiflikDurumu,BaslangicTarihi,BitisTarihi")] KullaniciRol kullaniciRol)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kullaniciRol);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KullaniciId"] = new SelectList(_context.Kullanicis, "Id", "Id", kullaniciRol.KullaniciId);
            ViewData["RolId"] = new SelectList(_context.Rols, "Id", "Id", kullaniciRol.RolId);
            return View(kullaniciRol);
        }

        // GET: KullaniciRols/Edit/5/2
        public async Task<IActionResult> Edit(int? kullaniciId, int? rolId)
        {
            if (kullaniciId == null || rolId == null)
            {
                return NotFound();
            }

            var kullaniciRol = await _context.KullaniciRols.FindAsync(kullaniciId, rolId);
            if (kullaniciRol == null)
            {
                return NotFound();
            }
            ViewData["KullaniciId"] = new SelectList(_context.Kullanicis, "Id", "Id", kullaniciRol.KullaniciId);
            ViewData["RolId"] = new SelectList(_context.Rols, "Id", "Id", kullaniciRol.RolId);
            return View(kullaniciRol);
        }

        // POST: KullaniciRols/Edit/5/2
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int kullaniciId, int rolId, [Bind("KullaniciId,RolId,AktiflikDurumu,BaslangicTarihi,BitisTarihi")] KullaniciRol kullaniciRol)
        {
            if (kullaniciId != kullaniciRol.KullaniciId || rolId != kullaniciRol.RolId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kullaniciRol);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KullaniciRolExists(kullaniciRol.KullaniciId, kullaniciRol.RolId))
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
            ViewData["KullaniciId"] = new SelectList(_context.Kullanicis, "Id", "Id", kullaniciRol.KullaniciId);
            ViewData["RolId"] = new SelectList(_context.Rols, "Id", "Id", kullaniciRol.RolId);
            return View(kullaniciRol);
        }

        // GET: KullaniciRols/Delete/5/2
        public async Task<IActionResult> Delete(int? kullaniciId, int? rolId)
        {
            if (kullaniciId == null || rolId == null)
            {
                return NotFound();
            }

            var kullaniciRol = await _context.KullaniciRols
                .Include(k => k.Kullanici)
                .Include(k => k.Rol)
                .FirstOrDefaultAsync(m => m.KullaniciId == kullaniciId && m.RolId == rolId);
            if (kullaniciRol == null)
            {
                return NotFound();
            }

            return View(kullaniciRol);
        }

        // POST: KullaniciRols/Delete/5/2
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int kullaniciId, int rolId)
        {
            var kullaniciRol = await _context.KullaniciRols.FindAsync(kullaniciId, rolId);
            if (kullaniciRol != null)
            {
                _context.KullaniciRols.Remove(kullaniciRol);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KullaniciRolExists(int kullaniciId, int rolId)
        {
            return _context.KullaniciRols.Any(e => e.KullaniciId == kullaniciId && e.RolId == rolId);
        }
    }
}