using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Products
{
    public interface IProductTypeService
    {
        Task<ProductType> GetAsync(int Id);
        Task<int> SaveAsync(ProductType entity);
        Task<List<ProductType>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir);
    }
}
