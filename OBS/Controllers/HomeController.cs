using Microsoft.AspNetCore.Mvc;
using OBS.Models;
using System.Diagnostics;

namespace OBS.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("KullaniciOlustur", "Admin");
                else if (User.HasClaim("UserType", "Ogrenci"))
                    return RedirectToAction("Index", "Student");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
