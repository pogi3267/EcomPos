using ApplicationCore.Entities.Orders;
using ApplicationCore.Entities.Products;
using Infrastructure.Interfaces.Products;
using Infrastructure.Interfaces.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationWeb.Areas.Customer.Controller.APIs.UI
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;
        private readonly IProductService _productService;

        public CartController(ICartService service,
            IProductService productService)
        {
            _service = service;
            _productService = productService;
        }

        [HttpPost("add-cart")]
        public async Task<IActionResult> AddToCartAsync([FromForm] Carts req)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                Product product = await _productService.GetProductAsync(req.ProductId);
                if (product == null) return NotFound("Product not found!");

                Carts cart = new Carts();

                ProductStock productStock = product.ProductStocks.First(x => x.Variant == req.Variant && x.ProductId == req.ProductId);

                if (productStock == null)
                {
                    cart.Price = product.UnitPrice;
                }
                else
                {
                    if (productStock.Price <= 0)
                        cart.Price = product.UnitPrice;
                    else
                        cart.Price = productStock.Price;
                }

                Product productDiscount = await _productService.GetProductDiscountAsync(req.ProductId);
                cart.Discount = productDiscount.Discount;
                cart.Price = productDiscount.DiscountPrice;

                foreach (var productTax in product.ProductTaxes)
                {
                    if (productTax.TaxType == "percent")
                        cart.Tax += (cart.Price * productTax.Amount) / 100;
                    else if (productTax.TaxType == "amount")
                        cart.Tax += productTax.Amount;
                }

                if (product.MinQuantity > req.Quantity)
                {
                    return BadRequest($"Minimum {product.MinQuantity} item(s) should be ordered");
                }

                // Checking stock
                int cartQuantity = await _service.GetCartQuantityByProductAsync(userId, req.ProductId, req.Variant);

                var stockQty = productStock?.Quantity ?? 0;
                if (stockQty < (req.Quantity + cartQuantity))
                {
                    if (stockQty == 0)
                        return BadRequest("Stock out!");
                    else
                        return BadRequest($"Only {stockQty} item(s) are available for this item!");
                }

                cart.Variation = productStock?.Variant;
                cart.VariationId = productStock?.ProductStockId ?? 0;
                cart.Quantity = req.Quantity;
                cart.OwnerId = product.UserId;
                cart.ProductId = product.ProductId;
                cart.UserId = userId;
                cart.ShippingType = product.ShippingType;
                cart.ShippingCost = product.IsQuantityMultiplied == true ? (Convert.ToDecimal(product.ShippingCost) * Convert.ToDecimal(cart.Quantity)) : Convert.ToDecimal(product.ShippingCost);
                cart.PickupPoint = req.PickupPoint;
                cart.ProductReferralCode = req.ProductReferralCode;
                cart.CouponCode = req.CouponCode;
                cart.CouponApplied = req.CouponApplied;

                await _service.AddToCartAsync(cart);

                #region Cart List

                Dictionary<string, object> dict = new Dictionary<string, object>();
                List<dynamic> carts = await _service.GetCartListAsync(userId);

                dict.Add("CartList", carts);
                decimal subTotal = 0;
                carts.ForEach(x =>
                {
                    subTotal += x.Price * x.Quantity;
                });
                dict.Add("SubTotal", subTotal);
                return Ok(dict);

                #endregion Cart List
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("remove-cart")]
        public async Task<IActionResult> RemoveFromCartAsync([FromForm] Carts req)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                req.UserId = userId;
                await _service.RemoveFromCartAsync(req);

                #region Cart List

                Dictionary<string, object> dict = new Dictionary<string, object>();
                List<dynamic> carts = await _service.GetCartListAsync(userId);

                dict.Add("CartList", carts);
                decimal subTotal = 0;
                carts.ForEach(x =>
                {
                    subTotal += x.Price * x.Quantity;
                });
                dict.Add("SubTotal", subTotal);
                return Ok(dict);

                #endregion Cart List
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("cart-count")]
        public async Task<IActionResult> GetCartCountAsync()
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var data = await _service.GetCartCountAsync(userId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("cart-list")]
        public async Task<IActionResult> GetCartListAsync()
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                Dictionary<string, object> dict = new Dictionary<string, object>();
                List<dynamic> carts = await _service.GetCartListAsync(userId);

                dict.Add("CartList", carts);
                decimal subTotal = 0;
                carts.ForEach(x =>
                {
                    subTotal += x.UnitPrice * x.Quantity;
                });
                dict.Add("SubTotal", subTotal);
                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("add-wishlist")]
        public async Task<IActionResult> AddToWishlistAsync(int id)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                Product product = await _productService.GetProductAsync(id);
                if (product == null) return NotFound("Product not found!");
                await _service.AddToWishlistAsync(id, userId);

                var data = await _service.GetWishListCountAsync(userId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("remove-wishlist")]
        public async Task<IActionResult> RemoveFromWishlistAsync(int id)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                await _service.RemoveFromWishlistAsync(id, userId);

                var data = await _service.GetWishListCountAsync(userId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("wishlist-count")]
        public async Task<IActionResult> GetWishListCountAsync()
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var data = await _service.GetWishListCountAsync(userId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("wish-list")]
        public async Task<IActionResult> GetWishListAsync()
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                List<dynamic> wishList = await _service.GetWishListAsync(userId);

                return Ok(wishList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-promo")]
        public async Task<IActionResult> GetCouponByCodeAsync(string promo)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var coupon = await _service.GetCouponByCodeAsync(userId, promo);

                if (coupon == null) return NotFound("Invalid promo!");

                return Ok(coupon);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("buy-product")]
        public async Task<IActionResult> BuyProductAsync([FromForm] Carts req)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                Product product = await _productService.GetProductAsync(req.ProductId);
                if (product == null) return NotFound("Product not found!");

                Carts cart = new Carts();

                ProductStock productStock = product.ProductStocks.First(x => x.Variant == req.Variant && x.ProductId == req.ProductId);

                if (productStock == null)
                {
                    cart.Price = product.UnitPrice;
                }
                else
                {
                    if (productStock.Price <= 0)
                        cart.Price = product.UnitPrice;
                    else
                        cart.Price = productStock.Price;
                }

                Product productDiscount = await _productService.GetProductDiscountAsync(req.ProductId);
                cart.Discount = productDiscount.Discount;
                cart.Price = productDiscount.DiscountPrice;

                foreach (var productTax in product.ProductTaxes)
                {
                    if (productTax.TaxType == "percent")
                        cart.Tax += (cart.Price * productTax.Amount) / 100;
                    else if (productTax.TaxType == "amount")
                        cart.Tax += productTax.Amount;
                }

                if (product.MinQuantity > req.Quantity)
                {
                    return BadRequest($"Minimum {product.MinQuantity} item(s) should be ordered");
                }

                // Checking stock
                int cartQuantity = await _service.GetCartQuantityByProductAsync(userId, req.ProductId, req.Variant);

                var stockQty = productStock?.Quantity ?? 0;
                if (stockQty < (req.Quantity + cartQuantity))
                {
                    if (stockQty == 0)
                        return BadRequest("Stock out!");
                    else
                        return BadRequest($"Only {stockQty} item(s) are available for this item!");
                }

                cart.Variation = productStock?.Variant;
                cart.VariationId = productStock?.ProductStockId ?? 0;
                cart.Quantity = req.Quantity;
                cart.OwnerId = product.UserId;
                cart.ProductId = product.ProductId;
                cart.UserId = userId;
                cart.ShippingType = product.ShippingType;
                cart.ShippingCost = product.IsQuantityMultiplied == true ? (Convert.ToDecimal(product.ShippingCost) * Convert.ToDecimal(cart.Quantity)) : Convert.ToDecimal(product.ShippingCost);
                cart.PickupPoint = req.PickupPoint;
                cart.ProductReferralCode = req.ProductReferralCode;
                cart.CouponCode = req.CouponCode;
                cart.CouponApplied = req.CouponApplied;

                await _service.BuyProductAsync(cart);

                #region Cart List

                Dictionary<string, object> dict = new Dictionary<string, object>();
                List<dynamic> carts = await _service.GetBuyProductListAsync(userId);

                dict.Add("CartList", carts);
                decimal subTotal = 0;
                carts.ForEach(x =>
                {
                    subTotal += x.UnitPrice * x.Quantity;
                });
                dict.Add("SubTotal", subTotal);
                return Ok(dict);

                #endregion Cart List
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}