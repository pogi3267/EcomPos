using ApplicationCore.Entities.ApplicationUser;
using ApplicationCore.Entities.Orders;
using ApplicationCore.Entities.Products;
using ApplicationCore.Entities.SetupAndConfigurations;
using ApplicationCore.Models;
using ApplicationWeb.Areas.Admin.Controllers.APIs.Authentication;
using ApplicationWeb.HelperAndConstant;
using ApplicationWeb.Utility;
using Infrastructure.Interfaces.Products;
using Infrastructure.Interfaces.SetupAndConfigurations;
using Infrastructure.Interfaces.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.IdentityModel.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Image = SixLabors.ImageSharp.Image;

namespace ApplicationWeb.Areas.Customer.Controller.APIs.UI
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly IBusinessSettingService _businessService;
        private readonly IImageProcessing _image;

        public OrderController(IOrderService service,
            IProductService productService,
            ICartService cartService,
            IImageProcessing imageProcessing,
            IBusinessSettingService businessService)
        {
            _service = service;
            _productService = productService;
            _cartService = cartService;
            _businessService = businessService;
            _image = imageProcessing;
        }

        [HttpGet("address-list")]
        public async Task<IActionResult> GetAddressListAsync()
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                List<dynamic> addressList = await _service.GetAddressListAsync(userId);

                return Ok(addressList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("address")]
        public async Task<IActionResult> GetAddressAsync(int addressId)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                dynamic address = await _service.GetAddressAsync(addressId, userId);

                return Ok(address);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("save-address")]
        public async Task<IActionResult> SaveAddressAsync([FromForm] ApplicationCore.Entities.ApplicationUser.Address address)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                if (address.Country == "undefined") address.Country = "";
                if (address.State == "undefined") address.State = "";
                if (address.City == "undefined") address.City = "";

                await _service.SaveAddressAsync(address, userId);

                return Ok("Saved Successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete-address")]
        public async Task<IActionResult> DeleteAddressAsync(int addressId)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                await _service.DeleteAddressAsync(addressId, userId);
                return Ok("Deleted Successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("order")]
        public async Task<IActionResult> CreateOrderInCodeAsync(string cartIds, int addressId, int couponId)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var carts = await _cartService.GetCartListByIdsAsync(userId, cartIds);
                if (!carts.Any())
                {
                    return BadRequest("Your cart is empty!");
                }

                var address = await _service.GetAddressAsync(addressId, userId);
                if (address == null)
                {
                    return BadRequest("Please select an address!");
                }

                var combinedOrder = new CombinedOrder
                {
                    UserId = userId,
                    ShippingAddress = JsonConvert.SerializeObject(address)
                };

                var order = new ApplicationCore.Entities.Orders.Orders
                {
                    UserId = userId,
                    ShippingAddress = combinedOrder.ShippingAddress,
                    ShippingType = carts.First()["ShippingType"].ToString(),
                    DeliveryStatus = "Pending",
                    PaymentType = "Cash_On_Delivery", // Set your payment option here
                    PaymentStatus = "Cash on delivery",
                    DeliveryViewed = false,
                    PaymentStatusViewed = false,
                    Code = $"{DateTime.UtcNow:yyyyMMdd-HHmm}{new Random().Next(10, 99)}",
                    Date = DateTime.UtcNow
                };

                decimal subtotal = 0;
                decimal tax = 0;
                decimal discount = 0;
                decimal shippingCost = 0;

                List<OrderDetail> orderDetails = new List<OrderDetail>();
                Product product = new Product();
                foreach (var cart in carts)
                {
                    product = await _productService.GetProductAsync(Convert.ToInt32(cart["ProductId"]));
                    var productStock = product.ProductStocks.FirstOrDefault(ps => ps.ProductId == Convert.ToInt32(cart["ProductId"]) && ps.Variant == cart["Variant"].ToString());

                    if (productStock != null && productStock.Quantity < Convert.ToInt32(cart["Quantity"]))
                    {
                        return BadRequest($"The requested quantity is not available for {cart["Variant"].ToString()} of {product.Name}");
                    }
                    else if (productStock != null)
                    {
                        productStock.Quantity -= Convert.ToInt32(cart["Quantity"]);
                    }

                    OrderDetail orderDetail = new OrderDetail
                    {
                        SellerId = cart["OwnerId"].ToString(),
                        ProductId = cart["ProductId"].ToString(),
                        Variation = cart["Variant"].ToString(),
                        Price = Convert.ToDecimal(cart["TotalPrice"]), //Price * Quantity
                        Tax = Convert.ToDecimal(cart["Tax"]) * Convert.ToDecimal(cart["Quantity"]),
                        ShippingType = cart["ShippingType"].ToString(),
                        ProductReferralCode = cart["ProductReferralCode"].ToString(),
                        ShippingCost = cart["ShippingCost"].ToString(),
                        Discount = Convert.ToDecimal(cart["Discount"]) * Convert.ToDecimal(cart["Quantity"]),
                        Quantity = Convert.ToInt32(cart["Quantity"])
                    };
                    orderDetails.Add(orderDetail);

                    subtotal += orderDetail.Price;
                    tax += orderDetail.Tax;
                    shippingCost += orderDetail.ShippingCost;
                    discount += orderDetail.Discount;

                    product.NumberOfSale += orderDetail.Quantity;
                }

                order.GrandTotal = (subtotal + tax + shippingCost) - discount;

                if (couponId > 0)
                {
                    var coupon = await _cartService.GetCouponDetailsAsync(userId, couponId, order.GrandTotal);
                    if (coupon == null)
                        return BadRequest("Invalid promo code!");

                    order.CouponDiscount = Convert.ToDecimal(coupon["DiscountAmount"]);
                    order.GrandTotal -= Convert.ToDecimal(coupon["DiscountAmount"]);

                    dynamic couponUsage = new
                    {
                        UserId = userId,
                        CouponId = couponId
                    };
                }

                combinedOrder.GrandTotal += order.GrandTotal;

                return Ok("Saved Successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrderAsync(string cartIds, int addressId, int couponId, string paymentType, int pickupId)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var result = (IDictionary<string, object>)await _service.CreateOrderAsync(userId, cartIds, addressId, couponId, paymentType, pickupId);

                dynamic order = await _service.GetOrderAsync(Convert.ToInt32(result["OrderId"]));

                return Ok(order);
            }
            catch (Exception ex)
            {
                string firstLineOfError = ex.Message.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];

                return BadRequest(firstLineOfError);
            }
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrdersAsync()
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                List<dynamic> orders = await _service.GetMyOrdersAsync(userId);

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("order-details")]
        public async Task<IActionResult> GetOrderDetailsAsync(int id)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                dynamic order = await _service.GetOrderDetailsAsync(id);
                order.ShippingType = OrderHelper.GetShippingType(order.ShippingType);
                order.DeliveryStatus = OrderHelper.GetOrderStatus(order.DeliveryStatus);
                ApplicationCore.Entities.ApplicationUser.Address address = System.Text.Json.JsonSerializer.Deserialize<ApplicationCore.Entities.ApplicationUser.Address>(order.ShippingAddress.ToString());
                order.Address = address;
                order.Date = Convert.ToDateTime(order.Date);

                decimal summaryPrice = 0, summaryDiscount = 0, summaryTax = 0, summaryShippingCost = 0, summarySubTotal = 0, summaryCouponDiscount = 0, summaryTotal = 0;
                foreach (var detail in order.OrderDetailsList)
                {
                    summaryPrice += Convert.ToDecimal(detail.Price);
                    summaryDiscount += Convert.ToDecimal(detail.Discount);
                    summaryTax += Convert.ToDecimal(detail.Tax);
                    summaryShippingCost += Convert.ToDecimal(detail.ShippingCost);
                    //summarySubTotal += (Convert.ToDecimal(detail.Price) + Convert.ToDecimal(detail.Tax) + Convert.ToDecimal(detail.ShippingCost)) - Convert.ToDecimal(detail.Discount);
                    summarySubTotal += (Convert.ToDecimal(detail.Price) + Convert.ToDecimal(detail.Tax) + Convert.ToDecimal(detail.ShippingCost));
                }
                summaryCouponDiscount += Convert.ToDecimal(order.CouponDiscount);
                summaryTotal += Convert.ToDecimal(order.GrandTotal);

                Dictionary<string, object> summary = new Dictionary<string, object>();
                summary["Price"] = summaryPrice;
                summary["Discount"] = summaryDiscount;
                summary["Tax"] = summaryTax;
                summary["ShippingCost"] = summaryShippingCost;
                summary["SubTotal"] = summarySubTotal;
                summary["CouponDiscount"] = summaryCouponDiscount;
                summary["Total"] = summaryTotal;
                summary["PaymentType"] = order.PaymentDetails;
                summary["PaidBy"] = order.PaymentStatus;
                summary["Rebate"] = order.TotalRebate;

                order.Summary = summary;

                foreach (var history in order.StatusHistory)
                {
                    if (history.CurrentStatus == "OutForDelivery") history.CurrentStatus = "Out For Delivery";
                    else if (history.CurrentStatus == "FailedToDeliver") history.CurrentStatus = "Failed To Deliver";
                    else if (history.CurrentStatus == "Canceled") history.CurrentStatus = "Cancelled";
                }

                var addressParts = new List<string>();
                if (!string.IsNullOrEmpty(address.ReceiverName))
                    addressParts.Add(address.ReceiverName);

                if (!string.IsNullOrEmpty(address.Phone))
                    addressParts.Add(address.Phone);

                if (!string.IsNullOrEmpty(address.Email))
                    addressParts.Add(address.Email);

                if (!string.IsNullOrEmpty(address.Address1))
                    addressParts.Add(address.Address1);

                if (!string.IsNullOrEmpty(address.City))
                    addressParts.Add(address.City);

                if (!string.IsNullOrEmpty(address.State))
                    addressParts.Add(address.State);

                if (!string.IsNullOrEmpty(address.Country))
                    addressParts.Add(address.Country);

                order.AddressInString = string.Join(", ", addressParts);

                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("review-list")]
        public async Task<IActionResult> GetReviewListAsync(int orderId)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                List<dynamic> reviewList = await _service.GetReviewListAsync(userId, orderId);
                foreach (dynamic review in reviewList)
                {
                    review.ReviewPhotos = review.ReviewPhotos.Split(',');
                }

                return Ok(reviewList);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("save-review")]
        public async Task<IActionResult> SaveReviewAsync([FromForm] Reviews review)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                List<string> fileNames = new List<string>();
                const int maxSizeInBytes = 512 * 1024; // 512 KB limit
                if (review.ReviewImages.Count > 0)
                {
                    foreach (var item in review.ReviewImages)
                    {
                        if (item.Length > maxSizeInBytes)
                        {
                            return BadRequest("One or more images exceed. The maximum allowed size of 0.5MB/512 KB.");
                        }

                        if (item.Length < 50 * 1024)
                        {
                            string imageExtension = item.FileName.Substring(item.FileName.LastIndexOf("."));
                            string fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + "_Image" + imageExtension;
                            string imagePath = _image.GetImagePath(fileName, "Images");
                            string dbPath = _image.GetImagePathForDb(imagePath);

                            using (var fileStream = new FileStream(imagePath, FileMode.Create))
                            {
                                await item.CopyToAsync(fileStream);
                            }

                            fileNames.Add(dbPath);
                        }
                        else
                        {
                            string imageExtension = item.FileName.Substring(item.FileName.LastIndexOf("."));
                            string fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + "_Image" + imageExtension;
                            string imagePath = _image.GetImagePath(fileName, "Images");
                            string dbPath = _image.GetImagePathForDb(imagePath);

                            using (var image = await Image.LoadAsync(item.OpenReadStream()))
                            {
                                int quality = 90;
                                using (var memoryStream = new MemoryStream())
                                {
                                    while (true)
                                    {
                                        memoryStream.Position = 0;
                                        memoryStream.SetLength(0);

                                        var encoder = new JpegEncoder { Quality = quality };
                                        await image.SaveAsync(memoryStream, encoder);

                                        if (memoryStream.Length <= 100 * 1024 || quality <= 10)
                                        {
                                            break;
                                        }

                                        quality -= 5;
                                    }

                                    using (var fileStream = new FileStream(imagePath, FileMode.Create))
                                    {
                                        memoryStream.Position = 0;
                                        await memoryStream.CopyToAsync(fileStream);
                                    }
                                }
                            }

                            fileNames.Add(dbPath);
                        }
                    }
                    review.ReviewPhotos = string.Join(",", fileNames.Select(x => x));
                }

                await _service.SaveReviewAsync(review, userId);

                return Ok(new { message = "Saved Successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("shipping-cost")]
        public async Task<IActionResult> GetShippingCostAsync(string cartIds, string? cityId)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                dynamic cost = await _service.GetShippingCostAsync(userId, cartIds, cityId);

                return Ok(cost);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("paid-order")]
        public async Task<IActionResult> PaidOrderStatusAsync(int orderId)
        {
            try
            {
                if (orderId <= 0) return BadRequest(new { error = "Invalid order!" });

                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                await _service.PaidOrderStatusAsync(orderId);

                return Ok(new { message = "Saved Successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("cod-order")]
        public async Task<IActionResult> MakeOrderCODAsync(int orderId)
        {
            try
            {
                if (orderId <= 0) return BadRequest(new { error = "Invalid order!" });

                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                await _service.MakeOrderCODAsync(orderId, userId);

                return Ok(new { message = "Saved Successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("send-order-mail")]
        public async Task<IActionResult> SendOrderMailAsync(int id)
        {
            BusinessSetting data = await _businessService.GetAsync("send_mail_to_admin_after_order");
            bool IsNeededToMailAdmin = (data != null) ? Convert.ToBoolean(data.Value) : false;
            string adminMail = "";
            if (IsNeededToMailAdmin)
            {
                data = await _businessService.GetAsync("admin_mailing_address");
                adminMail = (data != null) ? data.Value : "";
            }

            data = await _businessService.GetAsync("send_mail_to_customer_after_order");
            bool IsNeededToMailCustomer = (data != null) ? Convert.ToBoolean(data.Value) : false;

            dynamic order = await _service.GetOrderDetailsAsync(id);
            var orderDict = order as IDictionary<string, object>;

            EmailHelper emailHelper = new EmailHelper();

            if (IsNeededToMailAdmin)
            {
                string subjet = $@"New Order Notification - Order {orderDict?["Code"]}";
                string body = $@"
                <p>Dear Admin,</p>

                <p>I hope this message finds you well. I am writing to inform you that we have received a new order through our e-commerce platform. Below are the details for your review:</p>

                <p><strong>Order Summary:</strong><br />
                ---------------------------------<br />
                Order Number: {orderDict?["Code"]}<br />
                Total: {orderDict?["GrandTotal"]}</p>

                <p>Please ensure that the order is processed accordingly. If you have any questions or need further information, feel free to reach out.</p>

                <p>Thank you for your attention to this order.</p>

                <p>Best regards,<br />
                Sensify</p>";


                bool emailResponse = emailHelper.SendEmailForOrder(adminMail, subjet, body);
            }


            if (IsNeededToMailCustomer)
            {
                string? email = orderDict?["Email"].ToString();
                if (orderDict?["ShippingAddress"] != null)
                {
                    ApplicationCore.Entities.ApplicationUser.Address address = System.Text.Json.JsonSerializer.Deserialize<ApplicationCore.Entities.ApplicationUser.Address>(orderDict?["ShippingAddress"].ToString());
                    if (!string.IsNullOrEmpty(address?.Email))
                        email = address.Email;
                }


                if (!string.IsNullOrEmpty(email))
                {
                    string subjet = $@"Your Order Confirmation - Order {orderDict?["Code"]}";
                    string body = $@"
                    <p>Dear {orderDict?["CustomerName"]},</p>

                    <p>Thank you for your order! We are excited to let you know that your order <strong>{orderDict?["Code"]}</strong> has been successfully placed and is being processed.</p>

                    <p><strong>Order Summary:</strong><br />
                    ---------------------------------<br />
                    Order Number: {orderDict?["Code"]}<br />
                    Total: {orderDict?["GrandTotal"]}</p>

                    <p>Thank you for shopping with us!</p>

                    <p>Best regards,<br />
                    Sensify</p>";


                    bool emailResponse = emailHelper.SendEmailForOrder(email, subjet, body);
                }

            }

            return Ok("Send Successfully!");
        }

        [AllowAnonymous]
        [HttpGet("order-details-v2")]
        public async Task<IActionResult> GetOrderDetailsV2Async(int id)
        {
            try
            {
                dynamic order = await _service.GetOrderDetailsAsync(id);
                order.ShippingType = OrderHelper.GetShippingType(order.ShippingType);
                order.DeliveryStatus = OrderHelper.GetOrderStatus(order.DeliveryStatus);
                ApplicationCore.Entities.ApplicationUser.Address address = System.Text.Json.JsonSerializer.Deserialize<ApplicationCore.Entities.ApplicationUser.Address>(order.ShippingAddress.ToString());
                order.Address = address;
                order.Date = Convert.ToDateTime(order.Date);

                decimal summaryPrice = 0, summaryDiscount = 0, summaryTax = 0, summaryShippingCost = 0, summarySubTotal = 0, summaryCouponDiscount = 0, summaryTotal = 0;
                foreach (var detail in order.OrderDetailsList)
                {
                    summaryPrice += Convert.ToDecimal(detail.Price);
                    summaryDiscount += Convert.ToDecimal(detail.Discount);
                    summaryTax += Convert.ToDecimal(detail.Tax);
                    summaryShippingCost += Convert.ToDecimal(detail.ShippingCost);
                    //summarySubTotal += (Convert.ToDecimal(detail.Price) + Convert.ToDecimal(detail.Tax) + Convert.ToDecimal(detail.ShippingCost)) - Convert.ToDecimal(detail.Discount);
                    summarySubTotal += (Convert.ToDecimal(detail.Price) + Convert.ToDecimal(detail.Tax) + Convert.ToDecimal(detail.ShippingCost));
                }
                summaryCouponDiscount += Convert.ToDecimal(order.CouponDiscount);
                summaryTotal += Convert.ToDecimal(order.GrandTotal);

                Dictionary<string, object> summary = new Dictionary<string, object>();
                summary["Price"] = summaryPrice;
                summary["Discount"] = summaryDiscount;
                summary["Tax"] = summaryTax;
                summary["ShippingCost"] = summaryShippingCost;
                summary["SubTotal"] = summarySubTotal;
                summary["CouponDiscount"] = summaryCouponDiscount;
                summary["Total"] = summaryTotal;
                summary["PaymentType"] = order.PaymentDetails;
                summary["PaidBy"] = order.PaymentStatus;
                summary["Rebate"] = order.TotalRebate;

                order.Summary = summary;

                foreach (var history in order.StatusHistory)
                {
                    if (history.CurrentStatus == "OutForDelivery") history.CurrentStatus = "Out For Delivery";
                    else if (history.CurrentStatus == "FailedToDeliver") history.CurrentStatus = "Failed To Deliver";
                    else if (history.CurrentStatus == "Canceled") history.CurrentStatus = "Cancelled";
                }

                var addressParts = new List<string>();
                if (!string.IsNullOrEmpty(address.ReceiverName))
                    addressParts.Add(address.ReceiverName);

                if (!string.IsNullOrEmpty(address.Phone))
                    addressParts.Add(address.Phone);

                if (!string.IsNullOrEmpty(address.Email))
                    addressParts.Add(address.Email);

                if (!string.IsNullOrEmpty(address.Address1))
                    addressParts.Add(address.Address1);

                if (!string.IsNullOrEmpty(address.City))
                    addressParts.Add(address.City);

                if (!string.IsNullOrEmpty(address.State))
                    addressParts.Add(address.State);

                if (!string.IsNullOrEmpty(address.Country))
                    addressParts.Add(address.Country);

                order.AddressInString = string.Join(", ", addressParts);

                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}