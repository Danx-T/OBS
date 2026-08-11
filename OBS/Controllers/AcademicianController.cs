using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;
using OBS.ViewModels.Academician;
using System.Security.Claims;

namespace OBS.Controllers
{
    [Authorize]
    public class AcademicianController : Controller
    {
        private readonly ObsContext _context;

        public AcademicianController(ObsContext context)
        {
            _context = context;
        }

        private async Task<OgretimUyesi?> GetCurrentAcademicianAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return null;

            return await _context.OgretimUyesis
                .Include(ou => ou.Kullanici)
                .Include(ou => ou.Organizasyon)
                .FirstOrDefaultAsync(ou => ou.KullaniciId == userId);
        }

        private async Task<Donem?> GetActiveSemesterAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _context.Donems
                .FirstOrDefaultAsync(d => d.BaslangicTarihi <= today && d.BitisTarihi >= today);
        }

        // GET: /Academician/Index (Dashboard)
        public async Task<IActionResult> Index()
        {
            var academician = await GetCurrentAcademicianAsync();
            if (academician == null) return RedirectToAction("Login", "Auth");

            var activeSemester = await GetActiveSemesterAsync();
            
            var todaysClasses = new List<DersProgrami>();
            if (activeSemester != null)
            {
                var dayMapping = new Dictionary<DayOfWeek, string>
                {
                    { DayOfWeek.Monday, "Pazartesi" },
                    { DayOfWeek.Tuesday, "Salı" },
                    { DayOfWeek.Wednesday, "Çarşamba" },
                    { DayOfWeek.Thursday, "Perşembe" },
                    { DayOfWeek.Friday, "Cuma" },
                    { DayOfWeek.Saturday, "Cumartesi" },
                    { DayOfWeek.Sunday, "Pazar" }
                };
                var todayStr = dayMapping[DateTime.Today.DayOfWeek];

                todaysClasses = await _context.DersProgramis
                    .Include(dp => dp.AcilanDers)
                        .ThenInclude(ad => ad.Ders)
                    .Include(dp => dp.Salon)
                    .Where(dp => dp.Gun == todayStr && 
                                 dp.AcilanDers.DonemId == activeSemester.Id &&
                                 dp.AcilanDers.OgretimUyesiId == academician.Id)
                    .OrderBy(dp => dp.BaslangicSaati)
                    .ToListAsync();
            }

            var vm = new AcademicianDashboardViewModel
            {
                OgretimUyesi = academician,
                AktifDonem = activeSemester,
                BugunkuDersler = todaysClasses
            };

            return View(vm);
        }

    }
}
