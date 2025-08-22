using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Products
{
    public interface IBrandService
    {
        Task<Brand> GetAsync(int Id);
        Task<int> SaveAsync(Brand entity);
        Task<List<Brand>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir);
    }
}
