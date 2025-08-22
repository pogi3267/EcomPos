using ApplicationCore.Entities.SetupAndConfigurations;
using ApplicationCore.Models;
using Infrastructure.Interfaces.Authentication;
using Infrastructure.Interfaces.SetupAndConfigurations;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Text.Json;

namespace ApplicationWeb.Areas.Admin.Controllers.APIs.Authentication
{
    [Area("Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;
        private readonly IBusinessSettingService _businessService;

        public PaymentController(
            IPaymentService service,
            IBusinessSettingService businessService)
        {
            _service = service;
            _businessService = businessService;
        }

        [HttpPost]
        [Route("stripe-payment")]
        public async Task<ActionResult> CreateStripePaymentAsync([FromBody] CreatePaymentRequest request)
        {
            try
            {
                BusinessSetting businessSetting = await _businessService.GetAsync("stripe_payment");

                using JsonDocument document = JsonDocument.Parse(businessSetting.Value);
                JsonElement root = document.RootElement;

                string? stripeKey = root.GetProperty("stripeSecret").GetString();

                if (string.IsNullOrEmpty(stripeKey))
                {
                    return BadRequest(new { error = "Configure secret key!" });
                }

                var paymentIntent = await _service.CreateStripePaymentAsync(request, stripeKey);

                return Ok(new { clientSecret = paymentIntent.ClientSecret });
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = e.Message });
            }
        }

        [HttpPost]
        [Route("create-stripe-payment")]
        public async Task<ActionResult> CreatePaymentIntentAsync([FromForm] CreatePaymentRequest request)
        {
            try
            {
                BusinessSetting businessSetting = await _businessService.GetAsync("stripe_payment");

                using JsonDocument document = JsonDocument.Parse(businessSetting.Value);
                JsonElement root = document.RootElement;

                string? stripeKey = root.GetProperty("stripeSecret").GetString();

                if (string.IsNullOrEmpty(stripeKey))
                {
                    return BadRequest(new { error = "Configure secret key!" });
                }

                var paymentIntent = await _service.CreateStripePaymentAsync(request, stripeKey);

                return Ok(new { clientSecret = paymentIntent.ClientSecret });
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = e.Message });
            }
        }
    }
}