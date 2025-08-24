using ApplicationCore.Entities;
using ApplicationCore.Entities.Products;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Products;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ApplicationWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly IMenuMasterService _menuService;
        private readonly IProductService _productService;

        public ProductsController(IMenuMasterService menuService, IProductService productService)
        {
            _menuService = menuService;
            _productService = productService;
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

        public async Task<IActionResult> PrintBarcode(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View();
        }

        public async Task<IActionResult> TestBarcode(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View();
        }

        public async Task<IActionResult> SimpleBarcodeTest(int id)
        {
            MenuMaster menu = await _menuService.GetMenubyId(id);
            ViewBag.MenuId = menu.MenuMasterId;
            ViewBag.PageName = menu.PageName;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsForBarcode()
        {
            try
            {
                var products = await _productService.GetProductsWithVariantsForBarcode();
                return Json(products);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchProductsForBarcode(string searchTerm)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm) || searchTerm.Length < 2)
                {
                    return Json(new List<object>());
                }

                var products = await _productService.SearchProductsWithVariantsForBarcode(searchTerm);
                return Json(products);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductById(int productId)
        {
            try
            {
                var product = await _productService.GetProductWithVariantsForBarcode(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found" });
                }
                
                return Json(new { success = true, product = product });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateBarcode([FromBody] Infrastructure.Interfaces.Products.BarcodeRequestModel request)
        {
            try
            {
                if (request?.Products == null || !request.Products.Any())
                {
                    return Json(new { success = false, message = "No products selected" });
                }

                // Generate barcode logic here
                var barcodeResult = await _productService.GenerateBarcode(request);
                
                return Json(new { 
                    success = true, 
                    message = "Barcode generated successfully",
                    printUrl = barcodeResult.PrintUrl,
                    downloadUrl = barcodeResult.DownloadUrl
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PrintBarcodePreview(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                {
                    return RedirectToAction("PrintBarcode");
                }

                var barcodeRequest = JsonSerializer.Deserialize<Infrastructure.Interfaces.Products.BarcodeRequestModel>(data);
                ViewBag.BarcodeData = barcodeRequest;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadBarcode(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                {
                    return RedirectToAction("PrintBarcode");
                }

                var barcodeRequest = JsonSerializer.Deserialize<Infrastructure.Interfaces.Products.BarcodeRequestModel>(data);
                
                // Generate PDF or image file for download
                var barcodeResult = await _productService.GenerateBarcode(barcodeRequest);
                
                if (barcodeResult.Success && barcodeResult.BarcodeData != null)
                {
                    return File(barcodeResult.BarcodeData, "application/pdf", "barcode.pdf");
                }
                
                return RedirectToAction("PrintBarcode");
            }
            catch (Exception ex)
            {
                return RedirectToAction("PrintBarcode");
            }
        }

    }
}


