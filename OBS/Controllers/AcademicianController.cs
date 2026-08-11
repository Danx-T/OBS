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

        // GET: /Academician/Courses (Derslerim)
        public async Task<IActionResult> Courses()
        {
            var academician = await GetCurrentAcademicianAsync();
            if (academician == null) return RedirectToAction("Login", "Auth");

            var activeSemester = await GetActiveSemesterAsync();
            
            var courses = new List<AcilanDer>();
            if (activeSemester != null)
            {
                courses = await _context.AcilanDers
                    .Include(ad => ad.Ders)
                    .Include(ad => ad.DersKaydis) // To get student count
                    .Where(ad => ad.OgretimUyesiId == academician.Id && ad.DonemId == activeSemester.Id)
                    .OrderBy(ad => ad.Ders.DersKodu)
                    .ToListAsync();
            }

            var vm = new AcademicianCoursesViewModel
            {
                OgretimUyesi = academician,
                AktifDonem = activeSemester,
                VerilenDersler = courses
            };

            return View(vm);
        }

        // GET: /Academician/CourseStudents/{id} (Dersi Alan Öğrenciler)
        public async Task<IActionResult> CourseStudents(int id)
        {
            var academician = await GetCurrentAcademicianAsync();
            if (academician == null) return RedirectToAction("Login", "Auth");

            var acilanDers = await _context.AcilanDers
                .Include(ad => ad.Ders)
                .Include(ad => ad.Donem)
                .FirstOrDefaultAsync(ad => ad.Id == id && ad.OgretimUyesiId == academician.Id);

            if (acilanDers == null)
            {
                return NotFound();
            }

            // Get students who have "Onaylandi" or any valid status for this course
            var dersKayitlari = await _context.DersKaydis
                .Include(dk => dk.Ogrenci)
                    .ThenInclude(o => o.Kullanici)
                .Include(dk => dk.Ogrenci)
                    .ThenInclude(o => o.Organizasyon)
                .Where(dk => dk.AcilanDersId == id && dk.KayitDurumu == "Onaylandi")
                .OrderBy(dk => dk.Ogrenci.OgrenciNo)
                .ToListAsync();

            var vm = new AcademicianCourseStudentsViewModel
            {
                OgretimUyesi = academician,
                AcilanDers = acilanDers,
                DersKayitlari = dersKayitlari
            };

            return View(vm);
        }

        // --- ADVISOR PORTAL ---
        
        // GET: /Academician/AdvisorApprovals
        public async Task<IActionResult> AdvisorApprovals()
        {
            var academician = await GetCurrentAcademicianAsync();
            if (academician == null) return RedirectToAction("Login", "Auth");

            var activeSemester = await GetActiveSemesterAsync();
            if (activeSemester == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Danışmanı olduğu öğrencilerin aktif dönemdeki ders kayıtları
            var ogrenciKayitlari = await _context.DersKaydis
                .Include(dk => dk.Ogrenci)
                    .ThenInclude(o => o.Kullanici)
                .Include(dk => dk.AcilanDers)
                    .ThenInclude(ad => ad.Ders)
                .Include(dk => dk.AcilanDers)
                    .ThenInclude(ad => ad.DersProgramis)
                        .ThenInclude(dp => dp.Salon)
                .Where(dk => dk.Ogrenci.DanismanId == academician.Id && dk.AcilanDers.DonemId == activeSemester.Id)
                .ToListAsync();

            var vm = new AcademicianAdvisorViewModel
            {
                OgretimUyesi = academician,
                AktifDonem = activeSemester
            };

            var grouped = ogrenciKayitlari.GroupBy(dk => dk.OgrenciId).ToList();

            foreach (var group in grouped)
            {
                var student = group.First().Ogrenci;
                var records = group.ToList();

                // Eğer taslağın içinde en az bir tane "Onay Bekliyor" varsa genel durum bekliyordur
                var dto = new AdvisorStudentRegistrationDto
                {
                    Ogrenci = student,
                    DersKayitlari = records
                };

                if (records.Any(r => r.KayitDurumu == "Onay Bekliyor"))
                {
                    dto.GenelDurum = "Onay Bekliyor";
                    vm.OnayBekleyenler.Add(dto);
                }
                else if (records.All(r => r.KayitDurumu == "Reddedildi"))
                {
                    dto.GenelDurum = "Reddedildi";
                    vm.Reddedilenler.Add(dto);
                }
                else if (records.Any(r => r.KayitDurumu == "Onaylandi"))
                {
                    dto.GenelDurum = "Onaylandi";
                    vm.Onaylananlar.Add(dto);
                }
                else
                {
                    // "Kesinlestirildi" veya "Kontrol Edildi" durumları (Taslak) 
                    // henüz onaya gönderilmediği için hocanın listesine düşmemeli veya ayrı düşmeli.
                    // Şimdilik Onay Bekleyenlere dahil etmiyoruz.
                }
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveStudentRegistration(int ogrenciId)
        {
            var activeSemester = await GetActiveSemesterAsync();
            var academician = await GetCurrentAcademicianAsync();
            if (activeSemester == null || academician == null) return RedirectToAction(nameof(Index));

            var records = await _context.DersKaydis
                .Include(dk => dk.Ogrenci)
                .Include(dk => dk.AcilanDers)
                .Where(dk => dk.OgrenciId == ogrenciId && 
                             dk.Ogrenci.DanismanId == academician.Id && 
                             dk.AcilanDers.DonemId == activeSemester.Id &&
                             dk.KayitDurumu == "Onay Bekliyor")
                .ToListAsync();

            foreach (var record in records)
            {
                record.KayitDurumu = "Onaylandi";
                record.OnayTarihi = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AdvisorApprovals));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectStudentRegistration(int ogrenciId)
        {
            var activeSemester = await GetActiveSemesterAsync();
            var academician = await GetCurrentAcademicianAsync();
            if (activeSemester == null || academician == null) return RedirectToAction(nameof(Index));

            var records = await _context.DersKaydis
                .Include(dk => dk.Ogrenci)
                .Include(dk => dk.AcilanDers)
                .Where(dk => dk.OgrenciId == ogrenciId && 
                             dk.Ogrenci.DanismanId == academician.Id && 
                             dk.AcilanDers.DonemId == activeSemester.Id &&
                             dk.KayitDurumu == "Onay Bekliyor")
                .ToListAsync();

            foreach (var record in records)
            {
                record.KayitDurumu = "Reddedildi";
                record.OnayTarihi = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AdvisorApprovals));
        }
    }
}
