using ApplicationCore.DTOs;
using ApplicationCore.Entities.Marketing;
using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Public
{
    public interface IGeneralService
    {
        Task<dynamic> GetSystemSettingAsync();
        Task<dynamic> GetOrderSettingAsync();
        Task<dynamic> GetTermsAndPrivacyAsync();
        Task<List<Category>> GetCategoriesAsync();
        Task<List<dynamic>> GetBrandsAsync();
        Task<List<Category>> GetCategoryListAsync();
        Task<List<dynamic>> GetProductStockListByProductIdsAsync(string productIds);
        Task<List<dynamic>> GetAttributesAsync();
        Task<List<dynamic>> GetFlashDealProductsAsync(int id, int noOfRows, string userId = null);
        Task<List<dynamic>> GetTodaysDealProductsAsync(int noOfRows, string userId = null);
        Task<List<dynamic>> GetTrendingProductsAsync(int noOfRows, string userId = null);
        Task<List<dynamic>> GetDiscountProductsAsync(int noOfRows, string userId = null);
        Task<List<dynamic>> GetFeaturedProductsAsync(int noOfRows, string userId = null);
        Task<List<dynamic>> GetRelatedProductsAsync(int id, int noOfRows, string userId = null);
        Task<List<dynamic>> GetCategoryWiseProductsAsync(int categoryId, int noOfRows, string userId = null);
        Task<List<dynamic>> GetTopCategoryProductsAsync(bool isFirst);
        Task<List<dynamic>> GetCustomerReviewsAsync();
        Task<int> SaveSpecialOfferEmailAsync(SpecialOfferEmails entity);
        Task<dynamic> GetProductDetailsAsync(int productId, string userId = null);
        Task<dynamic> GetProductDetailsForSEOAsync(int productId);
        Task<List<dynamic>> GetFilterProductsAsync(ProductSearchDTO productSearch, string userId = null);
        Task<dynamic> GetFooterInfoAsync();
        Task<List<dynamic>> GetPickupPointListAsync();
        Task<List<dynamic>> GetCountryListAsync();
        Task<List<dynamic>> GetStateListAsync();
        Task<List<dynamic>> GetCityListAsync();
        Task<int> CreateUserActivities(SearchActivity entity);
        Task<List<string>> GetSearchResustAsync(string keySearch);
        Task ActionToReviewAsync(int id, int action, string userId);
    }
}