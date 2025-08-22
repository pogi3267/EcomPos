using ApplicationCore.Entities;
using ApplicationCore.Entities.Inventory;
using ApplicationCore.Entities.SetupAndConfigurations;
using ApplicationCore.Utilities;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Inventory;
using Infrastructure.Interfaces.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace EcomarceOnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class InventoryController : Controller
    {
        private readonly IMenuMasterService _menuService;
        private readonly ICategoryService _categoryService;
        private readonly IPurchaseReturnService _service;

        public InventoryController(IPurchaseReturnService purchaseService,IMenuMasterService menuService, ICategoryService categoryService)
        {
            _service = purchaseService;
            _menuService = menuService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> CreateSupplier(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View("CreateOperationalUser", SetDefaultInformation(OperationalUserRole.Supplier, "Supplier List", "Supplier"));
        }
        public async Task<IActionResult> CreateCustomer(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View("CreateOperationalUser", SetDefaultInformation(OperationalUserRole.Customer, "Customer List", "Customer"));
        }
        public async Task<IActionResult> CreateItem(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Item());
        }
        public async Task<IActionResult> Purchase(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Purchase());
        } 
        public async Task<IActionResult> PurchaseReturn(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new PurchaseReturn());
        }
        public async Task<IActionResult> Salse(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Salse());
        } 
        public async Task<IActionResult> SalesReturn(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new SalseReturn());
        }

        public async Task<IActionResult> Collection(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);

            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View();
        }
        public async Task<IActionResult> CollectionDeposite(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);

            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View();
        }
        public async Task<IActionResult> CollectionFinalized(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);

            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View();
        }

        public async Task<IActionResult> Payment(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);

            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View();
        }

        public async Task<IActionResult> Adjustment(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);

            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Adjustment());
        }
        public async Task<IActionResult> PaymentFinalized(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);

            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View(new Payment());
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> PurchaseReturnLoad(int purchaseId)
        {
            try
            {
                PurchaseReturn data = new PurchaseReturn();
                data.PurchaseId = purchaseId;

                return View("PurchaseReturn",data);
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SalseReturnLoad(int salseId)
        {
            try
            {
                SalseReturn data = new SalseReturn();
                data.SaleId = salseId;

                return View("SalesReturn", data);
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }
        }

        private static OperationalUser SetDefaultInformation(string roleName, string listName, string createNew)
        {
            return new OperationalUser()
            {
                Role = roleName,
                ListName = listName,
                CreateNew = createNew
            };
        }
    }
}