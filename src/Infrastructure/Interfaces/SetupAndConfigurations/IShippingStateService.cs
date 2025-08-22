using ApplicationCore.Entities.SetupAndConfigurations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.SetupAndConfigurations
{
    public interface IShippingStateService
    {
        Task<State> GetAsync(int Id);
        Task<State> GetInitial();
        Task<int> SaveAsync(State entity);
        Task<List<State>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir);

    }
}
