using ApplicationCore.Entities.SetupAndConfigurations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.SetupAndConfigurations
{
    public interface IPickupPointsService
    {
        Task<PickupPoint> GetNewAsync();
        Task<PickupPoint> GetAsync(int Id);
        Task<int> SaveAsync(PickupPoint entity);
        Task<List<PickupPoint>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir);
    }
}
