using ApplicationCore.DTOs;
using ApplicationCore.Entities;
using ApplicationWeb.ReportDataTable;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;
using System.Data;

namespace ApplicationWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class ReportsController : Controller
    {
        private readonly IMenuMasterService _menuService;
        private readonly IProductReportsService _reportService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ReportService _mainreportService;

        public ReportsController(IProductReportsService reportService, IWebHostEnvironment webHostEnvironment, IMenuMasterService menuService, ReportService mainreportService)
        {
            _webHostEnvironment = webHostEnvironment;
            _menuService = menuService;
            _reportService = reportService;
            _mainreportService = mainreportService;
        }


        public async Task<IActionResult> InhouseReport(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            ReportsDTO data = await _reportService.GetAsync();
            return View(data);
        }
        public async Task<IActionResult> ProductSalesReport(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            ReportsDTO data = await _reportService.GetAsync();
            return View(data);
        }
        public async Task<IActionResult> UserSearchReport(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            ReportsDTO data = await _reportService.GetAsync();
            return View(data);
        }
        public async Task<IActionResult> ProductWishListReport(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            ReportsDTO data = await _reportService.GetAsync();
            return View(data);
        }
        public async Task<IActionResult> ProductStockReport(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            if (menu.ParamValue == "Approve") ViewBag.ApprovePage = 1;
            else ViewBag.ApprovePage = 0;

            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            ReportsDTO data = await _reportService.GetAsync();
            return View(data);
        }


        public async Task<IActionResult> InhouseProduct(ReportsDTO reportsDTO)
        {
            List<ReportsDTO> inhouseProduct = await _reportService.GetInhouseProduct(reportsDTO);
            var dataTable = Helpers.ListToDataTable(inhouseProduct);
            string dataSet = "InhousrProductDS";
            var parameters = new List<ReportParameter> { };

            byte[] reportBytes = _mainreportService.RenderReport(dataSet, "InHouseProductSales.rdlc", "PDF", parameters, dataTable);

            return File(reportBytes, "application/pdf");
        }
        public async Task<IActionResult> ProductSales(ReportsDTO reportsDTO)
        {
            List<ReportsDTO> productSales = await _reportService.GetProductSales(reportsDTO);
            var dataTable = Helpers.ListToDataTable(productSales);
            string dataSet = "ProductSales";
            var parameters = new List<ReportParameter> { };

            byte[] reportBytes = _mainreportService.RenderReport(dataSet, "ProductSales.rdlc", "PDF", parameters, dataTable);

            return File(reportBytes, "application/pdf");
        }

        public async Task<IActionResult> GetUserSearchList(ReportsDTO reportsDTO)
        {
            List<ReportsDTO> productSales = await _reportService.GetUserSearchList(reportsDTO);
            var dataTable = Helpers.ListToDataTable(productSales);
            string dataSet = "DataSet1";
            var parameters = new List<ReportParameter>
        {
            new ReportParameter("FromDate", Convert.ToString(reportsDTO.FromDate)),
            new ReportParameter("ToDate", Convert.ToString(reportsDTO.ToDate))

        };

            byte[] reportBytes = _mainreportService.RenderReport(dataSet, "UserSearch.rdlc", "PDF", parameters, dataTable);

            return File(reportBytes, "application/pdf");
        }

        public async Task<IActionResult> GetProductWishList(ReportsDTO reportsDTO)
        {
            List<ReportsDTO> productSales = await _reportService.GetProductWishList(reportsDTO);
            var dataTable = Helpers.ListToDataTable(productSales);
            string dataSet = "WishListDS";
            var parameters = new List<ReportParameter> { };

            byte[] reportBytes = _mainreportService.RenderReport(dataSet, "WishListReport.rdlc", "PDF", parameters, dataTable);

            return File(reportBytes, "application/pdf");
        }
        public async Task<IActionResult> GetProductStock(ReportsDTO reportsDTO)
        {
            List<ReportsDTO> productSales = await _reportService.GetProductStock(reportsDTO);
            var dataTable = Helpers.ListToDataTable(productSales);
            string dataSet = "ProductStockDS";
            var parameters = new List<ReportParameter> { };

            byte[] reportBytes = _mainreportService.RenderReport(dataSet, "ProductStock.rdlc", "PDF", parameters, dataTable);

            return File(reportBytes, "application/pdf");
        }

        public async Task<IActionResult> GetInvoice(int id)
        {
            if (id > 0)
            {
                InvoiceOrderDTO invoiceOrder = await _reportService.GetInvoiceAsync(id);
                var dataTable1 = Helpers.ToDataTable(invoiceOrder); // Shipping and customer details

                List<InvoiceOrderDetail> invoiceOrderDetail = invoiceOrder.OrderDetails;
                var dataTable2 = Helpers.ListToDataTable(invoiceOrderDetail); // Product details
                GeneralSettings generalSettings = invoiceOrder.CompanyInfo;
                var dataTable3 = Helpers.ToDataTable(generalSettings); // Product details

                string dataSet1 = "ShippingAndCustomerDS";
                string dataSet2 = "ProductDetails";
                string dataSet3 = "DataSet1";

                var parameters = new List<ReportParameter> { };

                Dictionary<string, DataTable> dataSets = new Dictionary<string, DataTable>
    {
        { dataSet1, dataTable1 },
        { dataSet2, dataTable2 },
        { dataSet3, dataTable3 }
    };

                byte[] reportBytes = _mainreportService.RenderReport("Invoice.rdlc", "PDF", parameters, dataSets);

                return File(reportBytes, "application/pdf");
            }
            return File(new byte[0], "application/octet-stream", "");

        }

    }
}

