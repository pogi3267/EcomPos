using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Products
{
    public interface IProductStockService
    {
        Task<int> SaveMultipleAsync(IEnumerable<ProductStock> entity);
        Task<int> SaveSingleAsync(ProductStock entity);
        Task<List<ProductStock>> GetProductStock(int productId);
        Task<ProductStock> GetSingleStock(int productStockId);
    }
}
