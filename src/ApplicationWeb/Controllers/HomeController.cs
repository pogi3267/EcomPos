using ApplicationCore.Entities.ApplicationUser;
using ApplicationWeb.HelperAndConstant;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            try
            {
                var user = Global.GetCurrentUser();
                if (user != null) return View("Home");
                else
                    return RedirectToAction("Login", "Administration", new { Area = "Admin" });
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Administration", new { Area = "Admin" });
            }
        }

        public IActionResult DashBord()
        {
            return View("DashbordPartial");
        }
    }
}