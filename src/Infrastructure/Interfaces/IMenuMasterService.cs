using ApplicationCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IMenuMasterService
    {
        Task<List<MenuMaster>> GetMenuMaster(string userId);
        Task<List<MenuMaster>> GetMenusForClaims(string userId);
        Task<List<MenuMaster>> GetMenusForPermission(string userId);
        Task<MenuMaster> GetMenubyId(int id);
        Task<MenuMaster> GetMenuByPageName(string pageName);
        Task SaveMenuPermissionAsync(List<MenuMaster> menues, string userid);
    }
}
