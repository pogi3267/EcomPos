using ApplicationCore.Entities;
using ApplicationCore.Entities.Orders;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcomarceOnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrdersController : Controller
    {
        private readonly IMenuMasterService _menuService;

        public OrdersController(IMenuMasterService menuService)
        {
            _menuService = menuService;
        }

        public async Task<IActionResult> OrderList(int id, int orderId=0)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);

            ViewBag.ParamName = "all";
            if (menu.ParamValue == "Pending") ViewBag.ParamName = "Pending";
            else if (menu.ParamValue == "Confirmed") ViewBag.ParamName = "Confirmed";
            else if (menu.ParamValue == "Packaging") ViewBag.ParamName = "Packaging";
            else if (menu.ParamValue == "Out of delivery") ViewBag.ParamName = "Out of delivery";
            else if (menu.ParamValue == "Delivered") ViewBag.ParamName = "Delivered";
            else if (menu.ParamValue == "Return") ViewBag.ParamName = "Return";
            else if (menu.ParamValue == "Failed to deliver") ViewBag.ParamName = "Failed to deliver";
            else if (menu.ParamValue == "Cencelled") ViewBag.ParamName = "Cencelled";

            ViewBag.PickupOrder = false;
            if (menu.ParamValue == "PickupOrder") ViewBag.PickupOrder = true;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            ViewBag.DetailsForOrderId = orderId;
           
            return View(new Orders());
        }
    }
}