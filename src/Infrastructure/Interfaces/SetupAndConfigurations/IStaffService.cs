using ApplicationCore.Entities.SetupAndConfigurations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IStaffService
    {
        Task<Staff> GetNewAsync();
        Task<Staff> GetAsync(int Id);
        Task<int> SaveAsync(Staff entity);
        Task<List<Staff>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir);
    }
}