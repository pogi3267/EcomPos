using ApplicationCore.DTOs;
using CsvHelper;
using Infrastructure.Interfaces.Products;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace EcomarceOnlineShop.Areas.Admin.Controllers.APIs.Products
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize]
    public class BulkActionController : ControllerBase
    {
        private readonly IBulkActionService _service;

        public BulkActionController(IBulkActionService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("Upload")]
        public async Task<IActionResult> Upload(IFormFile uploadedFile)
        {
            try
            {

                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    using (var stream = new StreamReader(uploadedFile.OpenReadStream()))
                    using (var csvReader = new CsvReader(stream, CultureInfo.InvariantCulture))
                    {
                        var products = csvReader.GetRecords<BulkImport>().ToList();

                        await _service.SaveProductsToDatabase(products);
                    }

                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }

        [HttpGet]
        [Route("ExportCsv")]
        public async Task<IActionResult> ExportCsv()
        {
            try
            {
                var builder = new StringBuilder();
                builder.AppendLine("Name,Description,CategoryId,BrandId,VideoProvider,VideoLink,Tags,UnitPrice,UnitId,Slug,CurrentStock,EstShippingDays,ProductSKU,MetaDescription,ThumbnailImage,Photos");

                builder.AppendLine("Demo Product 22,Demo Product Description,1,1,youtube,youtube.com,Best Selling,120,1,demo-product-44,30,3,Demo Product 55,MetaDescription,https://Uploded/ThumbnailImage.jpeg,https://Image/Photos.jpeg");

                var csv = builder.ToString();
                var bytes = Encoding.UTF8.GetBytes(csv);

                return File(bytes, "text/csv", "Products.csv");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error occurred while exporting: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("ExportCategories")]
        public async Task<IActionResult> ExportCategories()
        {
            try
            {

                var cats = await _service.GetCategoriesAsync();

                var builder = new StringBuilder();
                builder.AppendLine("CategoryId, ParentId, Level, Name, OrderLevel, Banner, Icon, Featured, Digital, Slug, MetaTitle, MetaDescription");

                foreach (var cat in cats)
                {
                    builder.AppendLine($"{cat.CategoryId}, {cat.ParentId}, {cat.Level}, {cat.Name}, {cat.OrderLevel}, {cat.Banner}, {cat.Icon}, {cat.Featured}, {cat.Digital}, {cat.Slug}, {cat.MetaTitle}, {cat.MetaDescription}");
                }

                var csv = builder.ToString();
                var bytes = Encoding.UTF8.GetBytes(csv);

                return File(bytes, "text/csv", "Categories.csv");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error occurred while exporting: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("ExportBrands")]
        public async Task<IActionResult> ExportBrands()
        {
            try
            {
                var brands = await _service.GetBrandsAsync();

                var builder = new StringBuilder();
                builder.AppendLine("BrandId, Name, Logo, Top, Slug, MetaDescription, MetaTittle");

                foreach (var brand in brands)
                {
                    builder.AppendLine($"{brand.BrandId}, {brand.Name}, {brand.Logo}, {brand.Top}, {brand.Slug}, {brand.MetaDescription}, {brand.MetaTittle}");
                }

                var csv = builder.ToString();
                var bytes = Encoding.UTF8.GetBytes(csv);

                return File(bytes, "text/csv", "Brands.csv");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error occurred while exporting: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("ExportProducts")]
        public async Task<IActionResult> ExportProducts()
        {
            try
            {
                var products = await _service.GetProductsAsync();
                var builder = new StringBuilder();
                builder.AppendLine("Name,Description,CategoryId,BrandId,VideoProvider,VideoLink,Tags,UnitPrice,UnitId,Slug,CurrentStock,EstShippingDays,ProductSKU,MetaDescription,ThumbnailImage,Photos");
                foreach (var product in products)
                {
                    builder.AppendLine($"{product.Name},{product.Description},{product.CategoryId},{product.BrandId},{product.VideoProvider},{product.VideoLink},{product.Tags},{product.UnitPrice},{product.UnitId},{product.Slug},{product.CurrentStock},{product.EstShippingDays},{product.ProductSKU},{product.MetaDescription},{product.ThumbnailImage},{product.Photos}");
                }
                var csv = builder.ToString();
                var bytes = Encoding.UTF8.GetBytes(csv);

                return File(bytes, "text/csv", "AllProducts.csv");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error occurred while exporting: {ex.Message}");
            }
        }

    }
}