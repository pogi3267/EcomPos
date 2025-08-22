using ApplicationCore.Entities.Accounting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Accounting
{
    public interface IAccountLedgerService
    {
        Task<List<AccountLedger>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir);
        Task<AccountLedger> GetAsync(int id);
        Task<AccountLedger> GetAsync();
        Task<List<AccountLedger>> GetAccountLedgerByParentId(int parentId);
        Task<int> SaveAsync(AccountLedger entity);

        //Function AEL 2020
        Task<string>  CodeGenarator(string code);
        public int SaveChartOfAcc(AccountLedger objChartofAcc, int supplierId);
        public int UpdateChartAcc(string AccountCode, string AccountHead);
        public List<AccountLedger> LoadChartofaccounttree(string id);
        public List<AccountLedger> LoadParentNameCode(string ParentName);
        public List<AccountLedger> GetChartofAccList();
        public List<AccountLedger> ViewChartofAccData();
        public List<AccountLedger> ListChartofAccData();
        public AccountLedger LoadaChartofAccData(string invoice);
        public List<AccountLedger> GetLeadgrForPayVoucherSupName(int SupplierId);
        public List<AccountLedger> GetLeadgrForRcvVoucherSupName(int SupplierId);
        public List<AccountLedger> GetParentIdList(int ledgerId);
        Task<List<AccountLedger>>  GetParentLedgerByCode(string code);
        public int SaveChartOfCoa(List<AccountLedger> acclist, int intIdt);


        













        }
    }