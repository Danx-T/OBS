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
    public class KullaniciYetkisController : Controller
    {
        private readonly ObsContext _context;

        public KullaniciYetkisController(ObsContext context)
        {
            _context = context;
        }

        // GET: KullaniciYetkis
        public async Task<IActionResult> Index()
        {
            var obsContext = _context.KullaniciYetkis.Include(k => k.IslemYapanKullanici).Include(k => k.Kullanici).Include(k => k.Yetki);
            return View(await obsContext.ToListAsync());
        }

        // GET: KullaniciYetkis/Details/5/2/2026-01-01
        public async Task<IActionResult> Details(int? kullaniciId, int? yetkiId, DateTime? baslangicTarihi)
        {
            if (kullaniciId == null || yetkiId == null || baslangicTarihi == null)
            {
                return NotFound();
            }

            var kullaniciYetki = await _context.KullaniciYetkis
                .Include(k => k.IslemYapanKullanici)
                .Include(k => k.Kullanici)
                .Include(k => k.Yetki)
                .FirstOrDefaultAsync(m => m.KullaniciId == kullaniciId && m.YetkiId == yetkiId && m.BaslangicTarihi == baslangicTarihi);
            if (kullaniciYetki == null)
            {
                return NotFound();
            }

            return View(kullaniciYetki);
        }

        // GET: KullaniciYetkis/Create
        public IActionResult Create()
        {
            ViewData["IslemYapanKullaniciId"] = new SelectList(_context.Kullanicis, "Id", "Id");
            ViewData["KullaniciId"] = new SelectList(_context.Kullanicis, "Id", "Id");
            ViewData["YetkiId"] = new SelectList(_context.Yetkis, "Id", "Id");
            return View();
        }

        // POST: KullaniciYetkis/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("KullaniciId,YetkiId,IslemYapanKullaniciId,BaslangicTarihi,BitisTarihi")] KullaniciYetki kullaniciYetki)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kullaniciYetki);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IslemYapanKullaniciId"] = new SelectList(_context.Kullanicis, "Id", "Id", kullaniciYetki.IslemYapanKullaniciId);
            ViewData["KullaniciId"] = new SelectList(_context.Kullanicis, "Id", "Id", kullaniciYetki.KullaniciId);
            ViewData["YetkiId"] = new SelectList(_context.Yetkis, "Id", "Id", kullaniciYetki.YetkiId);
            return View(kullaniciYetki);
        }

        // GET: KullaniciYetkis/Edit/5/2/2026-01-01
        public async Task<IActionResult> Edit(int? kullaniciId, int? yetkiId, DateTime? baslangicTarihi)
        {
            if (kullaniciId == null || yetkiId == null || baslangicTarihi == null)
            {
                return NotFound();
            }

            var kullaniciYetki = await _context.KullaniciYetkis.FindAsync(kullaniciId, yetkiId, baslangicTarihi);
            if (kullaniciYetki == null)
            {
                return NotFound();
            }
            ViewData["IslemYapanKullaniciId"] = new SelectList(_context.Kullanicis, "Id", "Id", kullaniciYetki.IslemYapanKullaniciId);
            ViewData["KullaniciId"] = new SelectList(_context.Kullanicis, "Id", "Id", kullaniciYetki.KullaniciId);
            ViewData["YetkiId"] = new SelectList(_context.Yetkis, "Id", "Id", kullaniciYetki.YetkiId);
            return View(kullaniciYetki);
        }

        // POST: KullaniciYetkis/Edit/5/2/2026-01-01
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int kullaniciId, int yetkiId, DateTime baslangicTarihi, [Bind("KullaniciId,YetkiId,IslemYapanKullaniciId,BaslangicTarihi,BitisTarihi")] KullaniciYetki kullaniciYetki)
        {
            if (kullaniciId != kullaniciYetki.KullaniciId || yetkiId != kullaniciYetki.YetkiId || baslangicTarihi != kullaniciYetki.BaslangicTarihi)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kullaniciYetki);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KullaniciYetkiExists(kullaniciYetki.KullaniciId, kullaniciYetki.YetkiId, kullaniciYetki.BaslangicTarihi))
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
            ViewData["IslemYapanKullaniciId"] = new SelectList(_context.Kullanicis, "Id", "Id", kullaniciYetki.IslemYapanKullaniciId);
            ViewData["KullaniciId"] = new SelectList(_context.Kullanicis, "Id", "Id", kullaniciYetki.KullaniciId);
            ViewData["YetkiId"] = new SelectList(_context.Yetkis, "Id", "Id", kullaniciYetki.YetkiId);
            return View(kullaniciYetki);
        }

        // GET: KullaniciYetkis/Delete/5/2/2026-01-01
        public async Task<IActionResult> Delete(int? kullaniciId, int? yetkiId, DateTime? baslangicTarihi)
        {
            if (kullaniciId == null || yetkiId == null || baslangicTarihi == null)
            {
                return NotFound();
            }

            var kullaniciYetki = await _context.KullaniciYetkis
                .Include(k => k.IslemYapanKullanici)
                .Include(k => k.Kullanici)
                .Include(k => k.Yetki)
                .FirstOrDefaultAsync(m => m.KullaniciId == kullaniciId && m.YetkiId == yetkiId && m.BaslangicTarihi == baslangicTarihi);
            if (kullaniciYetki == null)
            {
                return NotFound();
            }

            return View(kullaniciYetki);
        }

        // POST: KullaniciYetkis/Delete/5/2/2026-01-01
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int kullaniciId, int yetkiId, DateTime baslangicTarihi)
        {
            var kullaniciYetki = await _context.KullaniciYetkis.FindAsync(kullaniciId, yetkiId, baslangicTarihi);
            if (kullaniciYetki != null)
            {
                _context.KullaniciYetkis.Remove(kullaniciYetki);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KullaniciYetkiExists(int kullaniciId, int yetkiId, DateTime baslangicTarihi)
        {
            return _context.KullaniciYetkis.Any(e => e.KullaniciId == kullaniciId && e.YetkiId == yetkiId && e.BaslangicTarihi == baslangicTarihi);
        }
    }
}