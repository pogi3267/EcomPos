using ApplicationCore.Entities.ApplicationUser;
using System.Threading.Tasks;

namespace ApplicationCore.Interfaces
{
    public interface IUserService
    {
        Task<dynamic> GetUserInfoAsync(string userId);
        Task SaveProfileAsync(User model);
    }
}