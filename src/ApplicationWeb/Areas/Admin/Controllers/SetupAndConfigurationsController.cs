using ApplicationCore.Entities;
using ApplicationCore.Entities.SetupAndConfigurations;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Products;
using Microsoft.AspNetCore.Mvc;

namespace EcomarceOnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SetupAndConfigurationsController : Controller
    {
        private readonly IMenuMasterService _menuService;
        private readonly ICategoryService _categoryService;

        public SetupAndConfigurationsController(IMenuMasterService menuService, ICategoryService categoryService)
        {
            _menuService = menuService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> CurrencyIndex(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Currency());
        }

        public async Task<IActionResult> VatAndTaxIndex(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Tax());
        }

        public async Task<IActionResult> PickupPointsIndex(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new PickupPoint());
        }

        public async Task<IActionResult> StaffIndex(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Staff());
        }

        public async Task<IActionResult> GeneralSettingsIndex(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new GeneralSettings());
        }

        public async Task<IActionResult> ShippingCountriesIndex(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Country());
        }

        public async Task<IActionResult> ShippingStateIndex(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new State());
        }

        public async Task<IActionResult> ShippingCitiesIndex(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new City());
        }

        public async Task<IActionResult> ShippingConfiguration(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View();
        }

        public async Task<IActionResult> PaymentMethodIndex(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View();
        }

        public async Task<IActionResult> PolicySetup(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View();
        }

        public async Task<IActionResult> TermsAndPrivacy(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View();
        }

        public async Task<IActionResult> BusinessSettings(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View();
        }

        public async Task<IActionResult> SocialLoginIndex(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View();
        }
    }
}