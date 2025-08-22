using ApplicationCore.Entities;
using ApplicationCore.Entities.Accounting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Accounting
{
    public interface IReceiveVoucherService
    {
        Task<List<AccountVoucher>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir);
        Task<AccountVoucher> GetInitial();
        Task<double> GetLedgerBalanceById(int accountLedgerId);
        Task<List<Select2OptionModel>> GetAccountHeadBySupplierId(int supplierId);
        Task<int> SaveAsync(AccountVoucher entity);
        Task<AccountVoucher> UpdateAsync(AccountVoucher entity);
        Task<string> InvoiceGenerate();
        Task<AccountVoucher> GetAsync(int id);
        Task<int> DeleteAsync(int id, string userId);
    }
}
