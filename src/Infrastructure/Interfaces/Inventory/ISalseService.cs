using ApplicationCore.Entities.Inventory;
using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Inventory
{
    public interface ISalseService
    {
        Task<Salse> GetInitial();
        Task<List<AttributeValue>> GetAttribute(string attributeList);
        Task<int> SaveAsync(Salse entity, List<SalseItem> salseItem, List<ProductStock> prodStocks);
        Task<ProductStock> GetProductStockAsync(int productId, string variant);
        Task UpdateProductStockAsync(ProductStock productStock);
        Task<List<Salse>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir, string status);
        Task<Salse> GetAsync(int id);
        Task<Salse> GetSalseAsync(int Id);
        Task<int> UpdateAsync(Salse entity,List<ProductStock> prodStocks);
        Task<List<SalseItem>> GetSalseItemsBySalseIdAsync(int salseId);
         Task<Salse> GetProductVariantList(int id);
        Task<ProductStock> GetStockValue(string productId, string variantName);

        //Task<Salse> GetAsync(int id);
        //Task<Salse> GetAllRelatedDataAsync(int salseId);
        //Task<Salse> GetSalseAsync(int salseId);
        //Task<string> GetLastInvoiceNumberAsync();
    }
}
