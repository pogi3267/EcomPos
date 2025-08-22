using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Products
{
    public interface IReviewsService
    {
        Task<Reviews> GetAsync(int Id);
        Task<Reviews> GetNewAsync();
        Task<int> SaveAsync(Reviews entity);
        Task<List<Reviews>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir);
        Task<List<Reviews>> GetListAsync(int pageNo = 0, int limit = 10, string filterBy = null, string orderBy = null);
    }
}
