using ApplicationCore.DTOs;
using ApplicationCore.Entities.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Infrastructure.Interfaces.Inventory
{
    public interface ICollectionService
    {
        Task<Collection> GetInitial();
        Task<dynamic> GetCustomerDueAmount(int customerId);
        Task<List<Collection>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir);
        Task<Collection> GetAsync(int id);
        Task<int> SaveAsync(Collection entity);
        Task<string> InvoiceGenerate();
        Task<OperationalUser> GetCustomerById(int customerId);
        Task<List<RepDateWiseCollectionDTO>> CollectionReportDateWise(CollectionReport collectionReport);
        Task<List<RepInvoiceCollectionDTO>> CollectionReportInvoiceWise(CollectionReport collectionReport);
    }
}
