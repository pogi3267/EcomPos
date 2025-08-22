using ApplicationCore.Entities;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcomarceOnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CustomerController : Controller
    {
        private readonly IMenuMasterService _menuService;

        public CustomerController(IMenuMasterService menuService)
        {
            _menuService = menuService;
        }

        public async Task<IActionResult> Customer(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);

            ViewBag.ParamName = "All";
            if (menu.ParamValue == "Active") ViewBag.ParamName = "Active";
            else if (menu.ParamValue == "Inactive") ViewBag.ParamName = "Inactive";

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View();
        }
    }
}