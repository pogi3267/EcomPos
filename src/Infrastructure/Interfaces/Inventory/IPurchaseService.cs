using ApplicationCore.Entities;
using ApplicationCore.Entities.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Inventory
{
    public interface IPurchaseService
    {
        Task<List<Purchase>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir, string status);
        Task<Purchase> GetInitial();
        Task<Purchase> GetPurchaseAsync(int id);
        Task<Purchase> GetPurchaseDetail(int id);
        Task<List<Select2OptionModel>> GetProductVariantsAsync(int productId);
        Task<int> SavePurchaseAsync(Purchase purchase, List<PurchaseItem> items, List<PurchaseExpense> expenses, List<PurchaseItem> oldItems);
    }
}
