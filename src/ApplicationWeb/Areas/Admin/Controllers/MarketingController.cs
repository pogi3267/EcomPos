using ApplicationCore.Entities;
using ApplicationCore.Entities.Marketing;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Products;
using Microsoft.AspNetCore.Mvc;

namespace EcomarceOnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MarketingController : Controller
    {
        private readonly IMenuMasterService _menuService;
        private readonly ICategoryService _categoryService;

        public MarketingController(IMenuMasterService menuService, ICategoryService categoryService)
        {
            _menuService = menuService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> FlashDeal(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new FlashDeal());
        }

        public async Task<IActionResult> Newsletter(int id)
        {
            await Task.Delay(0);
            return View();
        }

        public async Task<IActionResult> Subscribers(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View();
        }

        public async Task<IActionResult> Coupon(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Coupon());
        }

        public async Task<IActionResult> CreateFlashDeal(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new FlashDeal());
        }
    }
}