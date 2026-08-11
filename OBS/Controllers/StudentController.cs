using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OBS.Models;
using OBS.ViewModels.Student;
using System.Security.Claims;

namespace OBS.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly ObsContext _context;
        private readonly IMemoryCache _cache;

        public StudentController(ObsContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
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

        // --- DASHBOARD ---
        public async Task<IActionResult> Index()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null)
                return RedirectToAction("Login", "Auth"); 

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
                    .Where(dp => dp.Gun == todayStr && dp.AcilanDers.DonemId == activeSemester.Id)
                    .Where(dp => dp.AcilanDers.DersKaydis.Any(dk => dk.OgrenciId == student.Id && dk.KayitDurumu == "Onaylandi"))
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

        // --- COURSE REGISTRATION ---
        
        // GET: /Student/CourseRegistration
        public async Task<IActionResult> CourseRegistration()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Auth");

            var activeSemester = await GetActiveSemesterAsync();
            if (activeSemester == null)
            {
                TempData["Hata"] = "Şu anda aktif bir ders kayıt dönemi bulunmamaktadır.";
                return View(new CourseRegistrationViewModel { Ogrenci = student });
            }

            // 1. All available courses for this semester
            var availableCourses = await _context.AcilanDers
                .Include(ad => ad.Ders)
                .Include(ad => ad.OgretimUyesi)
                    .ThenInclude(ou => ou.Kullanici)
                .Include(ad => ad.DersProgramis)
                    .ThenInclude(dp => dp.Salon)
                .Where(ad => ad.DonemId == activeSemester.Id && ad.Durum == "Aktif" && ad.DersProgramis.Any())
                .ToListAsync();

            // 2. Existing records in DB for this student and semester
            var existingRegistrations = await _context.DersKaydis
                .Include(dk => dk.AcilanDers)
                    .ThenInclude(ad => ad.Ders)
                .Where(dk => dk.OgrenciId == student.Id && dk.AcilanDers.DonemId == activeSemester.Id)
                .ToListAsync();

            // 3. Courses in cache (selected but not finalized)
            var cacheKey = $"Student_{student.Id}_Cart";
            var cachedCourseIds = _cache.Get<List<int>>(cacheKey) ?? new List<int>();
            var cachedCourses = availableCourses.Where(ad => cachedCourseIds.Contains(ad.Id)).ToList();

            var vm = new CourseRegistrationViewModel
            {
                Ogrenci = student,
                AktifDonem = activeSemester,
                AcilanDersler = availableCourses,
                SecilenDersler = cachedCourses,
                MevcutKayitlar = existingRegistrations,
                HataMesaji = TempData["Hata"]?.ToString(),
                BasariMesaji = TempData["Basari"]?.ToString(),
                IsChecked = TempData["IsChecked"] != null && (bool)TempData["IsChecked"]
            };

            return View(vm);
        }

        // POST: /Student/AddCourseToCache
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourseToCache(int acilanDersId)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Auth");

            // Check if already in DB (ignore 'Reddedildi' so they can re-select)
            bool existsInDb = await _context.DersKaydis.AnyAsync(dk => dk.OgrenciId == student.Id && dk.AcilanDersId == acilanDersId && dk.KayitDurumu != "Reddedildi");
            if (existsInDb)
            {
                TempData["Hata"] = "Bu ders zaten kayıtlarınızda mevcut.";
                return RedirectToAction(nameof(CourseRegistration));
            }

            var cacheKey = $"Student_{student.Id}_Cart";
            var cachedCourseIds = _cache.Get<List<int>>(cacheKey) ?? new List<int>();

            if (!cachedCourseIds.Contains(acilanDersId))
            {
                cachedCourseIds.Add(acilanDersId);
                _cache.Set(cacheKey, cachedCourseIds, TimeSpan.FromHours(2));
            }

            return RedirectToAction(nameof(CourseRegistration));
        }

        // POST: /Student/RemoveCourseFromCache
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCourseFromCache(int acilanDersId)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Auth");

            var cacheKey = $"Student_{student.Id}_Cart";
            var cachedCourseIds = _cache.Get<List<int>>(cacheKey) ?? new List<int>();

            if (cachedCourseIds.Contains(acilanDersId))
            {
                cachedCourseIds.Remove(acilanDersId);
                _cache.Set(cacheKey, cachedCourseIds, TimeSpan.FromHours(2));
            }

            return RedirectToAction(nameof(CourseRegistration));
        }

        // POST: /Student/CheckCourses
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckCourses()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Auth");

            var activeSemester = await GetActiveSemesterAsync();
            if (activeSemester == null) return RedirectToAction(nameof(CourseRegistration));

            var cacheKey = $"Student_{student.Id}_Cart";
            var cachedCourseIds = _cache.Get<List<int>>(cacheKey) ?? new List<int>();

            if (!cachedCourseIds.Any())
            {
                TempData["Hata"] = "Kontrol edilecek ders bulunamadı.";
                return RedirectToAction(nameof(CourseRegistration));
            }

            var selectedPrograms = await _context.DersProgramis
                .Where(dp => cachedCourseIds.Contains(dp.AcilanDersId))
                .ToListAsync();

            var existingPrograms = await _context.DersProgramis
                .Where(dp => dp.AcilanDers.DonemId == activeSemester.Id && 
                             dp.AcilanDers.DersKaydis.Any(dk => dk.OgrenciId == student.Id && dk.KayitDurumu != "Reddedildi"))
                .ToListAsync();

            var allProgramsToCheck = selectedPrograms.Concat(existingPrograms).ToList();

            for (int i = 0; i < allProgramsToCheck.Count; i++)
            {
                for (int j = i + 1; j < allProgramsToCheck.Count; j++)
                {
                    var p1 = allProgramsToCheck[i];
                    var p2 = allProgramsToCheck[j];

                    if (p1.Gun == p2.Gun && p1.BaslangicSaati < p2.BitisSaati && p1.BitisSaati > p2.BaslangicSaati)
                    {
                        TempData["Hata"] = "Seçilen dersler arasında veya mevcut derslerinizle saat çakışması bulunmaktadır! Lütfen programı kontrol ediniz.";
                        return RedirectToAction(nameof(CourseRegistration));
                    }
                }
            }

            foreach (var dersId in cachedCourseIds)
            {
                var ders = await _context.AcilanDers.FindAsync(dersId);
                if (ders != null)
                {
                    int kayitliOgrenciSayisi = await _context.DersKaydis.CountAsync(dk => dk.AcilanDersId == dersId && dk.KayitDurumu != "Reddedildi");
                    if (kayitliOgrenciSayisi >= ders.Kontenjan)
                    {
                        TempData["Hata"] = "Bazı derslerin kontenjanı dolmuş. Lütfen kontrol ediniz.";
                        return RedirectToAction(nameof(CourseRegistration));
                    }
                }
            }

            TempData["IsChecked"] = true;
            TempData["Basari"] = "Seçilen derslerde çakışma veya kontenjan sorunu bulunamadı. Kesinleştirme işlemini yapabilirsiniz.";
            return RedirectToAction(nameof(CourseRegistration));
        }

        // POST: /Student/FinalizeRegistration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeRegistration()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Auth");

            var activeSemester = await GetActiveSemesterAsync();
            if (activeSemester == null) return RedirectToAction(nameof(CourseRegistration));

            var cacheKey = $"Student_{student.Id}_Cart";
            var cachedCourseIds = _cache.Get<List<int>>(cacheKey) ?? new List<int>();

            if (!cachedCourseIds.Any())
            {
                TempData["Hata"] = "Kesinleştirilecek ders bulunamadı.";
                return RedirectToAction(nameof(CourseRegistration));
            }

            // Double check inside finalize just in case they bypassed the UI
            var selectedPrograms = await _context.DersProgramis
                .Where(dp => cachedCourseIds.Contains(dp.AcilanDersId))
                .ToListAsync();

            var existingPrograms = await _context.DersProgramis
                .Where(dp => dp.AcilanDers.DonemId == activeSemester.Id && 
                             dp.AcilanDers.DersKaydis.Any(dk => dk.OgrenciId == student.Id && dk.KayitDurumu != "Reddedildi"))
                .ToListAsync();

            var allProgramsToCheck = selectedPrograms.Concat(existingPrograms).ToList();

            for (int i = 0; i < allProgramsToCheck.Count; i++)
            {
                for (int j = i + 1; j < allProgramsToCheck.Count; j++)
                {
                    var p1 = allProgramsToCheck[i];
                    var p2 = allProgramsToCheck[j];

                    if (p1.Gun == p2.Gun && p1.BaslangicSaati < p2.BitisSaati && p1.BitisSaati > p2.BaslangicSaati)
                    {
                        TempData["Hata"] = "Saat çakışması tespit edildi.";
                        return RedirectToAction(nameof(CourseRegistration));
                    }
                }
            }

            foreach (var dersId in cachedCourseIds)
            {
                var ders = await _context.AcilanDers.FindAsync(dersId);
                if (ders != null)
                {
                    int kayitliOgrenciSayisi = await _context.DersKaydis.CountAsync(dk => dk.AcilanDersId == dersId && dk.KayitDurumu != "Reddedildi");
                    if (kayitliOgrenciSayisi >= ders.Kontenjan)
                    {
                        TempData["Hata"] = "Bazı derslerin kontenjanı dolmuş.";
                        return RedirectToAction(nameof(CourseRegistration));
                    }

                    var existingKayit = await _context.DersKaydis.FirstOrDefaultAsync(dk => dk.OgrenciId == student.Id && dk.AcilanDersId == dersId);
                    if (existingKayit != null)
                    {
                        existingKayit.KayitDurumu = "Onay Bekliyor";
                        existingKayit.KayitTarihi = DateTime.Now;
                        existingKayit.OnayTarihi = null;
                    }
                    else
                    {
                        var kayit = new DersKaydi
                        {
                            OgrenciId = student.Id,
                            AcilanDersId = dersId,
                            KayitDurumu = "Onay Bekliyor",
                            KayitTarihi = DateTime.Now,
                            OnayTarihi = null
                        };
                        _context.DersKaydis.Add(kayit);
                    }
                }
            }

            await _context.SaveChangesAsync();
            _cache.Remove(cacheKey);

            TempData["Basari"] = "Ders kayıt işleminiz başarıyla kesinleştirildi ve danışman onayına sunuldu.";
            return RedirectToAction(nameof(CourseRegistration));
        }

        // POST: /Student/WithdrawCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WithdrawCourse(int dersKaydiId)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Auth");

            var kayit = await _context.DersKaydis
                .FirstOrDefaultAsync(dk => dk.Id == dersKaydiId && dk.OgrenciId == student.Id);

            if (kayit != null && kayit.KayitDurumu == "Onay Bekliyor")
            {
                _context.DersKaydis.Remove(kayit);
                await _context.SaveChangesAsync();
                TempData["Basari"] = "Onay bekleyen ders kaydınız başarıyla geri çekildi.";
            }
            else
            {
                TempData["Hata"] = "Bu ders kaydı geri çekilemez. Durumu değişmiş olabilir.";
            }

            return RedirectToAction(nameof(CourseRegistration));
        }

        // --- SCHEDULE ---
        public async Task<IActionResult> Schedule()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null)
                return RedirectToAction("Login", "Auth");

            var activeSemester = await GetActiveSemesterAsync();
            
            var schedule = new List<DersProgrami>();
            if (activeSemester != null)
            {
                schedule = await _context.DersProgramis
                    .Include(dp => dp.AcilanDers)
                        .ThenInclude(ad => ad.Ders)
                    .Include(dp => dp.Salon)
                    .Include(dp => dp.AcilanDers)
                        .ThenInclude(ad => ad.OgretimUyesi)
                            .ThenInclude(ou => ou.Kullanici)
                    .Where(dp => dp.AcilanDers.DonemId == activeSemester.Id)
                    .Where(dp => dp.AcilanDers.DersKaydis.Any(dk => dk.OgrenciId == student.Id && dk.KayitDurumu == "Onaylandi"))
                    .OrderBy(dp => dp.BaslangicSaati)
                    .ToListAsync();
            }

            var vm = new StudentScheduleViewModel
            {
                Ogrenci = student,
                AktifDonem = activeSemester,
                HaftalikDersProgrami = schedule
            };

            return View(vm);
        }

        // --- EXAMS ---
        public async Task<IActionResult> Exams()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null)
                return RedirectToAction("Login", "Auth");

            var activeSemester = await GetActiveSemesterAsync();
            
            var sinavlar = new List<SinavProgrami>();
            if (activeSemester != null)
            {
                sinavlar = await _context.SinavProgramis
                    .Include(sp => sp.AcilanDers)
                        .ThenInclude(ad => ad.Ders)
                    .Include(sp => sp.AcilanDers)
                        .ThenInclude(ad => ad.OgretimUyesi)
                            .ThenInclude(ou => ou.Kullanici)
                    .Include(sp => sp.Salon)
                    .Where(sp => sp.AcilanDers.DonemId == activeSemester.Id)
                    .Where(sp => sp.AcilanDers.DersKaydis.Any(dk => dk.OgrenciId == student.Id && dk.KayitDurumu == "Onaylandi"))
                    .OrderBy(sp => sp.AcilanDers.Ders.DersKodu)
                    .ThenBy(sp => sp.Baslangic)
                    .ToListAsync();
            }

            var vm = new StudentExamsViewModel
            {
                Ogrenci = student,
                AktifDonem = activeSemester,
                Sinavlar = sinavlar
            };

            return View(vm);
        }

        // --- GRADES ---
        public async Task<IActionResult> Grades()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null)
                return RedirectToAction("Login", "Auth");

            var activeSemester = await GetActiveSemesterAsync();
            
            var dersKayitlari = new List<DersKaydi>();
            if (activeSemester != null)
            {
                dersKayitlari = await _context.DersKaydis
                    .Include(dk => dk.AcilanDers)
                        .ThenInclude(ad => ad.Ders)
                    .Include(dk => dk.Notlars)
                    .Include(dk => dk.AcilanDers)
                        .ThenInclude(ad => ad.OgretimUyesi)
                            .ThenInclude(ou => ou.Kullanici)
                    .Where(dk => dk.OgrenciId == student.Id && 
                                 dk.AcilanDers.DonemId == activeSemester.Id && 
                                 dk.KayitDurumu == "Onaylandi") // Grades should only show for finalized courses
                    .OrderBy(dk => dk.AcilanDers.Ders.DersKodu)
                    .ToListAsync();
            }

            var vm = new StudentGradesViewModel
            {
                Ogrenci = student,
                AktifDonem = activeSemester,
                DersKayitlari = dersKayitlari
            };

            return View(vm);
        }
    }
}
