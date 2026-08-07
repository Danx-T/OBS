using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBS.Models;
using OBS.ViewModels.Student;
using System.Security.Claims;

namespace OBS.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly ObsContext _context;

        public StudentController(ObsContext context)
        {
            _context = context;
        }

        // Helper: Get Current User's Student Profile
        private async Task<Ogrenci?> GetCurrentStudentAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return null;

            return await _context.Ogrencis
                .Include(o => o.Danisman)
                    .ThenInclude(d => d.Kullanici)
                .Include(o => o.Kullanici)
                .FirstOrDefaultAsync(o => o.KullaniciId == userId);
        }

        // Helper: Get Active Semester
        private async Task<Donem?> GetActiveSemesterAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _context.Donems
                .FirstOrDefaultAsync(d => d.BaslangicTarihi <= today && d.BitisTarihi >= today);
        }

        public async Task<IActionResult> Index()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null)
                return RedirectToAction("Login", "Auth"); // Not a student or not found

            var activeSemester = await GetActiveSemesterAsync();
            
            // Get today's classes
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
                    .Where(dp => dp.Gun == todayStr && dp.AcilanDers.DonemId == activeSemester.Id)
                    .Where(dp => dp.AcilanDers.DersKaydis.Any(dk => dk.OgrenciId == student.Id && dk.KayitDurumu == "Onaylandı"))
                    .OrderBy(dp => dp.BaslangicSaati)
                    .ToListAsync();
            }

            var vm = new StudentDashboardViewModel
            {
                Ogrenci = student,
                AktifDonem = activeSemester,
                Danisman = student.Danisman,
                BugunkuDersler = todaysClasses
            };

            return View(vm);
        }
    }
}
