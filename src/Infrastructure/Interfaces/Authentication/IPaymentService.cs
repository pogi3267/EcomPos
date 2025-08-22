using ApplicationCore.Models;
using Stripe;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Authentication
{
    public interface IPaymentService
    {
        Task<PaymentIntent> CreateStripePaymentAsync(CreatePaymentRequest model, string stripeKey);
    }
}
