using ApplicationCore.Entities.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.UI
{
    public interface ICartService
    {
        Task<int> AddToCartAsync(Carts cart);
        Task<int> BuyProductAsync(Carts cart);
        Task<int> RemoveFromCartAsync(Carts cart);
        Task<dynamic> GetCartCountAsync(string user);
        Task<int> GetCartQuantityByProductAsync(string user, int prodId, string variant);
        Task<List<dynamic>> GetCartListAsync(string userid);
        Task<List<dynamic>> GetBuyProductListAsync(string userid);
        Task<List<dynamic>> GetCartListByIdsAsync(string userid, string cartIds);
        Task<dynamic> GetWishListCountAsync(string user);
        Task<int> AddToWishlistAsync(int productId, string userId);
        Task<int> RemoveFromWishlistAsync(int productId, string userId);
        Task<List<dynamic>> GetWishListAsync(string userid);
        Task<dynamic> GetCouponByCodeAsync(string userId, string promo);
        Task<dynamic> GetCouponDetailsAsync(string userId, int couponId, decimal amount);
    }
}