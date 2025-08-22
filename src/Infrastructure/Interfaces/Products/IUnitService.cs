using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Products
{
    public interface IUnitService
    {
        Task<Unit> GetAsync(int Id);
        Task<int> SaveAsync(Unit entity);
        Task<List<Unit>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir);
    }
}
