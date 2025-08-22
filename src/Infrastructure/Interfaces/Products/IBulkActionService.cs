using ApplicationCore.DTOs;
using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Products
{
    public interface IBulkActionService
    {
        Task SaveProductsToDatabase(List<BulkImport> product);
        Task<List<ApplicationCore.Entities.Products.Product>> GetProductsAsync();
        Task<List<Category>> GetCategoriesAsync();
        Task<List<Brand>> GetBrandsAsync();
    }
}
