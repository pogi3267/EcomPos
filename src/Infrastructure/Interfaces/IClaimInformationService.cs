using ApplicationCore.Entities.GeneralSettings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IClaimInformationService
    {
        Task<List<ClaimInformation>> GetListAsync();
    }
}