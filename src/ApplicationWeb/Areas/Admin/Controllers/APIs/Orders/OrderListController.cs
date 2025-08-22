using ApplicationCore.Entities.Orders;
using ApplicationCore.Entities.Products;
using ApplicationCore.Entities.SetupAndConfigurations;
using ApplicationCore.Models;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.Marketing;
using Infrastructure.Interfaces.Products;
using Infrastructure.Interfaces.SetupAndConfigurations;
using Infrastructure.Interfaces.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EcomarceOnlineShop.Areas.Admin.Controllers.APIs.Marketing
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize]
    public class OrderListController : ControllerBase
    {
        private readonly IOrderListService _service;
        private readonly IProductService _prodService;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IBusinessSettingService _businessService;

        public OrderListController(IOrderListService service, IProductService prodService, ICartService cartService, IOrderService orderService, IBusinessSettingService businessService)
        {
            _service = service;
            _prodService = prodService;
            _cartService = cartService;
            _orderService = orderService;
            _businessService = businessService;
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetListPostAsync()
        {
            var paginationResult = DataTableHandler.PaginationHandler(Request);
            string status = Request.Form["status"].ToString();
            bool isPickupOrder = Convert.ToBoolean(Request.Form["additionalparam1"].ToString());
            var result = await _service.GetList(paginationResult.Item2, paginationResult.Item3, paginationResult.Item4, paginationResult.Item5, paginationResult.Item6, status, isPickupOrder);
            int filteredResultsCount = result.Count > 0 ? result[0].TotalRows : 0;
            int totalResultsCount = result.Count > 0 ? result[0].TotalRows : 0;
            return Ok(new
            {
                paginationResult.Item1,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
            });
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            try
            {
                Orders data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");
                if (data.ShippingAddress != null)
                    data.Address = JsonSerializer.Deserialize<ApplicationCore.Entities.ApplicationUser.Address>(data.ShippingAddress);
                else
                    data.Address = new ApplicationCore.Entities.ApplicationUser.Address();

                data.ProductList = await _prodService.GetProductsToAddAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("update/{ordersId}/{paymentStatus}/{deliveryStatus}/{shippingType}/{courierName}/{courierTrackingNo}")]
        public async Task<IActionResult> SaveAsync(int ordersId, string paymentStatus, string? deliveryStatus, string? shippingType, string? courierName, string? courierTrackingNo)
        {
            try
            {
                Orders data = await _service.GetAsync(ordersId);
                bool isMailNeeded = false;
                if (data.DeliveryStatus != deliveryStatus)
                    isMailNeeded = true;

                Orders entity = new Orders();
                entity.OrdersId = ordersId;
                entity.PaymentStatus = paymentStatus;
                entity.DeliveryStatus = deliveryStatus;
                entity.ShippingType = shippingType == "undefined" ? "" : shippingType;
                entity.CourierName = (courierName == "undefined" || string.IsNullOrEmpty(courierName)) ? "" : courierName;
                entity.CourierTrackingNo = (courierTrackingNo == "undefined" || string.IsNullOrEmpty(courierTrackingNo)) ? "" : Uri.UnescapeDataString(courierTrackingNo);

                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();
                entity.UserId = userId;
                await _service.SaveAsync(entity);

                if (isMailNeeded)
                {
                    BusinessSetting bs = await _businessService.GetAsync("send_mail_after_change_order_status");
                    bool IsNeededToMail = (bs != null) ? Convert.ToBoolean(bs.Value) : false;
                    if (IsNeededToMail)
                    {
                        if (!string.IsNullOrEmpty(data.Email)) //need work
                        {
                            string courierInfo = "";
                            if (!string.IsNullOrEmpty(courierName)) courierInfo += "Courier Name: <strong>{courierName}</strong><br />";
                            if (!string.IsNullOrEmpty(courierTrackingNo)) courierInfo += "Tracking No: <strong>{courierTrackingNo}</strong><br />";

                            string subjet = $@"Your Order - Order {data.Code} Status Has Changed";
                            string body = $@"
                            <p>Dear {data.CustomerName},</p>

                            <p>We wanted to inform you that the status of your order <strong>{data.Code}</strong> has been updated. Below are the details of your order:</p>

                            <p><strong>Order Details:</strong><br />
                            ---------------------------------<br />
                            Order Number: {data.Code}<br />
                            Status: <strong>{deliveryStatus}</strong><br />
                            Total: {data.GrandTotal}<br />
                            {courierInfo}
                            </p>
                            <p>Thank you for shopping with us, and we hope to serve you again!</p>

                            <p>Best regards,<br />
                            Sensify</p>
                            ";

                            EmailHelper emailHelper = new EmailHelper();
                            bool emailResponse = emailHelper.SendEmailForOrder(data.Email, subjet, body);
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("DeleteNotification")]
        public async Task<IActionResult> DeleteNotificationAsync()
        {
            try
            {
                await _service.DeleteNotificationAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("order-product-details/{productId}/{variant}/{qty}")]
        public async Task<IActionResult> GetProductDetailsForOrderAsync(int productId, int variant, int qty)
        {
            try
            {
                ApplicationCore.Entities.Products.Product product = await _prodService.GetProductAsync(productId);
                if (product == null) return NotFound("Product not found!");
                ProductStock productStock = product.ProductStocks.First(x => x.ProductStockId == variant);
                Carts cart = new Carts();
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

                ApplicationCore.Entities.Products.Product productDiscount = await _prodService.GetProductDiscountAsync(productId);
                cart.Discount = productDiscount.Discount;
                cart.Price = productDiscount.DiscountPrice;

                foreach (var productTax in product.ProductTaxes)
                {
                    if (productTax.TaxType == "percent")
                        cart.Tax += (cart.Price * productTax.Amount) / 100;
                    else if (productTax.TaxType == "amount")
                        cart.Tax += productTax.Amount;
                }

                qty = qty <= 0 ? 1 : qty;
                var stockQty = productStock?.Quantity ?? 0;
                if (stockQty < qty)
                {
                    if (stockQty == 0)
                        return BadRequest("This item is stocked out!");
                    else
                        return BadRequest($"Only {stockQty} item(s) are available for this item!");
                }

                cart.Variation = productStock?.Variant;
                cart.VariationId = productStock?.ProductStockId ?? 0;
                cart.Quantity = qty;
                cart.OwnerId = product.UserId;
                cart.ProductId = product.ProductId;
                cart.ShippingType = product.ShippingType;
                decimal cost = await _orderService.GetShippingCostAsync(product.ProductId);
                cart.ShippingCost = product.IsQuantityMultiplied == true ? (Convert.ToDecimal(cost) * Convert.ToDecimal(cart.Quantity)) : Convert.ToDecimal(cost);

                OrderDetail detail = new OrderDetail();
                detail.ProductId = product.ProductId;
                detail.ProductName = product.Name;
                detail.ThumbnailImage = product.ThumbnailImagePath;
                detail.ProductPrice = product.UnitPrice;
                detail.Variation = cart.Variation;
                detail.VariantPrice = cart.Price ?? 0;
                detail.Price = (cart.Price ?? 0) * qty;
                detail.Tax = (cart.Tax ?? 0) * qty;
                detail.Discount = (cart.Discount ?? 0) * qty;
                detail.Quantity = cart.Quantity;
                detail.ShippingType = cart.ShippingType;
                detail.ShippingCost = cart.ShippingCost ?? 0;

                return Ok(detail);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("save-detail")]
        public async Task<IActionResult> SaveDetailAsync([FromForm] OrderDetail model)
        {
            try
            {
                await _service.SaveDetailAsync(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}