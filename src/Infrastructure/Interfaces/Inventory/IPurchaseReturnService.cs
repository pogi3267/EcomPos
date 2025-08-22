using ApplicationCore.Entities.Inventory;
using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Inventory
{
    public interface IPurchaseReturnService
    {
        Task<PurchaseReturn> GetInitial();
        Task<List<AttributeValue>> GetAttribute(string attributeList);
        Task<int> SaveAsync(PurchaseReturn entity, List<PurchaseReturnItem> purchaseItem, List<ProductStock> prodStocks);
        Task<ProductStock> GetProductStockAsync(int productId, string variant);
        Task UpdateProductStockAsync(ProductStock productStock);
        Task<List<PurchaseReturn>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir, string status);
        Task<PurchaseReturn> GetAsync(int id);
        Task<PurchaseReturn> GetPurchaseForEditAsync(int id);
        Task<List<ProductStock>> GetPurchaseStockAsync(int purchaseId);
        Task<List<ProductStock>> GetRtnEditStockAsync(int purchaseRetuenId);
        Task<PurchaseReturn> GetPurchaseAsync(int Id);
        Task<int> UpdateAsync(PurchaseReturn entity);
      
        Task<List<PurchaseReturnItem>> GetPurchaseItemsByPurchaseIdAsync(int purchaseId);


    }
}

