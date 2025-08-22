using ApplicationCore.Entities.Marketing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Marketing
{
    public interface ICouponService
    {
        Task<Coupon> GetAsync(int Id);
        Task<int> SaveAsync(Coupon entity);
        Task<List<Coupon>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir);
        Task<List<SpecialOfferEmails>> GetSubscriberListAsync(string searchBy, int take, int skip, string sortBy, string sortDir);
    }
}