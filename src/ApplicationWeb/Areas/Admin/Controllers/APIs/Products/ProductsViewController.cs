using ApplicationCore.Entities.Products;
using ApplicationWeb.Utility;
using Infrastructure.Interfaces.Products;
using Microsoft.AspNetCore.Mvc;

namespace EcomarceOnlineShop.Areas.Admin.Controllers.APIs.Products
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsViewController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductsViewController(IProductService service)
        {
            _service = service;
        }

        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart([FromForm] Product addToCartproduct)
        {
            try
            {
                List<Product> products = new List<Product>();
                var product = await _service.GetViewProductByIdAsync(addToCartproduct.ProductId);
                product.Colors = addToCartproduct.AddToCartColor;
                product.Attributes = addToCartproduct.AddToCartAttribute;
                product.AddToCartQuantity = addToCartproduct.AddToCartQuantity;

                if (products == null)
                {
                    return NotFound();
                }
                products = HttpContext.Session.Get<List<Product>>("products");
                if (products == null)
                {
                    products = new List<Product>();
                }
                products.Add(product);
                HttpContext.Session.Set("products", products);
                return Ok(products);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}