using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Products
{
    public interface IColorService
    {
        Task<Color> GetAsync(int Id);
        Task<Color> GetByCodeAsync(string code, int id);
        Task<int> SaveAsync(Color entity);
        Task<List<Color>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir);

    }
}
