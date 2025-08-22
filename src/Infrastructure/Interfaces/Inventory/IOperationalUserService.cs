using ApplicationCore.Entities.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Inventory
{
    public interface IOperationalUserService
    {
        Task<List<OperationalUser>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir, string roleName = null);
        Task<OperationalUser> GetAsync(int id);
        Task<int> SaveAsync(OperationalUser entity);
        Task<OperationalUser> GetInitial();
        Task<List<OperationalUser>> GetOperationalUserByRole(string roleName);
    }
}
