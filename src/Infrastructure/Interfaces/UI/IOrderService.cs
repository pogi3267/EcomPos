using ApplicationCore.Entities.ApplicationUser;
using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.UI
{
    public interface IOrderService
    {
        Task<List<dynamic>> GetAddressListAsync(string userid);
        Task<List<dynamic>> GetMyOrdersAsync(string userid);
        Task<dynamic> GetAddressAsync(int addressId, string userid);
        Task<dynamic> GetShippingCostAsync(string userid, string cartIds, string cityId);
        Task<decimal> GetShippingCostAsync(int productId = 0, string cityId = "0");
        Task<dynamic> GetOrderAsync(int orderId);
        Task SaveAddressAsync(Address address, string userid);
        Task SaveReviewAsync(Reviews review, string userid);
        Task DeleteAddressAsync(int addressId, string userid);
        Task<dynamic> CreateOrderAsync(string userId, string cartIds, int addressId, int couponId, string paymentType, int pickupId);
        Task<dynamic> CreateOrderV2Async(string userId, string productStocks, string address, int couponId, int paymentType, int pickupId, int shippingLocation);
        Task<dynamic> GetOrderDetailsAsync(int orderId);
        Task<List<dynamic>> GetReviewListAsync(string userid, int orderId);
        Task<bool> PaidOrderStatusAsync(int orderId);
        Task<bool> MakeOrderCODAsync(int orderId, string userId);
    }
}