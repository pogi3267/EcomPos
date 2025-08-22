using ApplicationCore.Entities.Inventory;
using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Inventory
{
    public interface IPurchaseService
    {
        Task<Purchase> GetInitial();
        Task<List<AttributeValue>> GetAttribute(string attributeList);
        Task<int> SaveAsync(Purchase entity, List<PurchaseItem> purchaseItem, List<ProductStock> prodStocks);
        Task<ProductStock> GetProductStockAsync(int productId, string variant);
        Task UpdateProductStockAsync(ProductStock productStock);
        Task<List<Purchase>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir, string status);
        Task<Purchase> GetAsync(int id);
        Task<Purchase> GetPurchaseAsync(int Id);
        Task<int> UpdateAsync(Purchase entity, List<ProductStock> productStocks);
        Task<List<PurchaseItem>> GetPurchaseItemsByPurchaseIdAsync(int purchaseId);

        //Task<Purchase> GetAsync(int id);
        //Task<Purchase> GetAllRelatedDataAsync(int purchaseId);
        //Task<Purchase> GetPurchaseAsync(int purchaseId);
        //Task<string> GetLastInvoiceNumberAsync();
    }
}
