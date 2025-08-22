using ApplicationCore.Entities.ApplicationUser;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface ICustomerService
    {
        Task<List<User>> GetList(string searchBy, int take, int skip, string sortBy, string sortDir, string status);
        Task<User> GetAsync(string Id);
    }
}
