using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Products
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategories();
        Task<Category> GetInitial();
        Task<Category> GetAsync(int Id);
        Task<int> SaveAsync(Category entity);
        Task<List<Category>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir);
    }
}
