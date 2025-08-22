using ApplicationCore.Entities;
using ApplicationCore.Entities.Products;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly IMenuMasterService _menuService;

        public ProductsController(IMenuMasterService menuService)
        {
            _menuService = menuService;
        }

        public async Task<IActionResult> ColorIndex(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Color());
        }

        public async Task<IActionResult> CategoryIndex(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Category());
        }

        public async Task<IActionResult> BrandIndex(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Brand());
        }

        public async Task<IActionResult> AttributeIndex(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new ProductAttribute());
        }

        public async Task<IActionResult> UnitIndex(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Unit());
        }

        public async Task<IActionResult> ProductTypeIndex(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new ProductType());
        }

        public async Task<IActionResult> CreateProduct(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Product());
        }
       
        public async Task<IActionResult> ProductList(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);

            ViewBag.ParamName = "All";
            if (menu.ParamValue == "Inhouse") ViewBag.ParamName = "Inhouse";

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View();
        }

        public async Task<IActionResult> ReviewsIndex(int id)
        {
            var menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Reviews());
        }
        public async Task<IActionResult> BulkImport(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Color());
        } 
        public async Task<IActionResult> BulkExport(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Color());
        }

    }
}


