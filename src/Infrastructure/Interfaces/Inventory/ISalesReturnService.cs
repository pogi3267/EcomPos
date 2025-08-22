using ApplicationCore.Entities.Inventory;
using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Inventory
{
    public interface ISalesReturnService
    {
        Task<SalseReturn> GetInitial();
        Task<List<AttributeValue>> GetAttribute(string attributeList);
        Task<int> SaveAsync(SalseReturn entity, List<SalseReturnItem> salseItem, List<ProductStock> prodStocks);
        Task<ProductStock> GetProductStockAsync(int productId, string variant);
        Task UpdateProductStockAsync(ProductStock productStock);
        Task<List<SalseReturn>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir, string status);
        Task<SalseReturn> SalesReturnLoad(int id);
        Task<SalseReturn> GetAsync(int id);
        Task<SalseReturn> GetSalseReturnAsync(int Id);
        Task<int> UpdateAsync(SalseReturn entity,List<ProductStock> prodStocks);
        Task<List<SalseReturnItem>> GetSalseReturnItemsBySalseReturnIdAsync(int salseId);
         Task<SalseReturn> GetProductVariantList(int id);

        //Task<SalseReturn> GetAsync(int id);
        //Task<SalseReturn> GetAllRelatedDataAsync(int salseId);
        //Task<SalseReturn> GetSalseReturnAsync(int salseId);
        //Task<string> GetLastInvoiceNumberAsync();
    }
}