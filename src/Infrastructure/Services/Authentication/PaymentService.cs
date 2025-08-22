using ApplicationCore.Models;
using Infrastructure.Interfaces.Authentication;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection;
        public PaymentService(IConfiguration configuration)
        {
            _configuration = configuration;
            var conStr = _configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(conStr);
        }

        public async Task<PaymentIntent> CreateStripePaymentAsync(CreatePaymentRequest request, string stripeKey)
        {
            try
            {
                StripeConfiguration.ApiKey = stripeKey;

                var options = new PaymentIntentCreateOptions
                {
                    Amount = request.Amount * 100,
                    Currency = "usd",
                    Metadata = new Dictionary<string, string>
                {
                    { "Quantity", "1" },
                    { "OrderId", request.OrderCode }
                }
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);
                return paymentIntent;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}