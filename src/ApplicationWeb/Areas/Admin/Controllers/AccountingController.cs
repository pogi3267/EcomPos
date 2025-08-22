using ApplicationCore.Entities;
using ApplicationCore.Entities.Accounting;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class AccountingController : Controller
{
    private readonly IMenuMasterService _menuService;
    public AccountingController(IMenuMasterService menuService)
    {
        _menuService = menuService;
    }

    public async Task<IActionResult> CreateChartofAccount(int id)
    {
        MenuMaster menu = await _menuService.GetMenubyId(id);
        if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
        else ViewBag.ApprovePage = 0;

        ViewBag.MenuId = menu.MenuMasterId;
        ViewBag.PageName = menu.PageName;
        return View();
    }
    public async Task<IActionResult> AddPaymentVoucher(int id)
    {
        MenuMaster menu = await _menuService.GetMenubyId(id);
        if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
        else ViewBag.ApprovePage = 0;

        ViewBag.MenuId = menu.MenuMasterId;
        ViewBag.PageName = menu.PageName;
        return View(new AccountVoucher());
    } 
    public async Task<IActionResult> ListPaymentVoucher(int id)
    {
        MenuMaster menu = await _menuService.GetMenubyId(id);
        if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
        else ViewBag.ApprovePage = 0;

        ViewBag.MenuId = menu.MenuMasterId;
        ViewBag.PageName = menu.PageName;
        return View(new AccountVoucher());
    }

    public async Task<IActionResult> AddReceiveVoucher(int id)
    {
        MenuMaster menu = await _menuService.GetMenubyId(id);
        if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
        else ViewBag.ApprovePage = 0;

        ViewBag.MenuId = menu.MenuMasterId;
        ViewBag.PageName = menu.PageName;
        return View();
    }
    public async Task<IActionResult> AddJournalVoucher(int id)
    {
        MenuMaster menu = await _menuService.GetMenubyId(id);
        if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
        else ViewBag.ApprovePage = 0;

        ViewBag.MenuId = menu.MenuMasterId;
        ViewBag.PageName = menu.PageName;
        return View();
    }
}


