using ApplicationCore.Entities;
using ApplicationCore.Entities.Accounting;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Accounting;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Infrastructure.Services.Accounting
{
    public class AccountLedgerService : IAccountLedgerService
    {
        private readonly IDapperService<AccountLedger> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction = null;

        public AccountLedgerService(IDapperService<AccountLedger> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<List<AccountLedger>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY Id DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT *, Count(*) Over() TotalRows FROM AccountLedger";
                if (searchBy != "")
                    sql += " WHERE ParentName like '%" + searchBy + "%' or Code like '%" + searchBy + "%' ";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<AccountLedger>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AccountLedger> GetAsync(int id)
        {
            try
            {
                var query = $@"SELECT Id, ParentId, ParentName, Code, Posted, LevelNo, RelatedId, RelatedIdFor, IsEditable, IsApproved, Common, Status, DeleteBy, DeleteDate, IsActive, IsDelete, ShowInIncomeStetement, Created_At, Updated_At, Created_By, Updated_By
                            FROM AccountLedger WHERE Id  = {id};";
                await _connection.OpenAsync();
                AccountLedger data = await _connection.QuerySingleAsync<AccountLedger>(query);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task<AccountLedger> GetAsync()
        {
            try
            {
                AccountLedger data = new AccountLedger();
                var query = $@"SELECT Id as Id, ParentName as Text,Code as Description 
                            FROM AccountLedger A WHERE A.Posted = 1;";
                var queryResult = await _connection.QueryMultipleAsync(query);
                data.ParentList = queryResult.Read<Select2OptionModel>().ToList();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> SaveAsync(AccountLedger entity)
        {
            try
            {
                await _connection.OpenAsync();
                _transaction = _connection.BeginTransaction();
                var id = await _service.SaveSingleAsync(entity, _transaction);
                _transaction.Commit();
                return id;
            }
            catch (Exception ex)
            {
                if (_transaction != null) _transaction.Rollback();
                throw ex;
            }
            finally
            {
                _transaction.Dispose();
                _connection.Close();
            }
        }



        //Function AEL 2020
        public async Task<string> CodeGenarator(string code)
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }

            var parameters = new DynamicParameters();
            parameters.Add("@InputCode", code);
            parameters.Add("@newInvoice", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

            _connection.Execute("AutoIDForChartofAcc", parameters, commandType: CommandType.StoredProcedure);

            string newInvoice = parameters.Get<string>("@newInvoice");

            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }

            return await Task.FromResult(newInvoice);
        }
        public int SaveChartOfAcc(AccountLedger objChartofAcc, int supplierId)
        {

            int ids = _service.GetAccountIdByCode(objChartofAcc.ParentAccountName);
            int ids2 = _service.GetAccountIdByCode(objChartofAcc.RootAccountName);


            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }

            try
            {
                string query = @"INSERT INTO AccountLedger(parent_id, related_id, related_id_for, code, parent_name, status, posted)
                        VALUES (@ParentAccount, @related_id, @related_id_for, @AccountCode, @AccountHead, 1, @Posted)";

                var parameters = new
                {
                    ParentAccount = (objChartofAcc.RootAccountName != "") && (objChartofAcc.ParentAccountName == "0") ? ids2 : ids,
                    AccountCode = objChartofAcc.Code,
                    AccountHead = objChartofAcc.ParentName,
                    related_id = supplierId,
                    related_id_for = 1,
                    Posted = objChartofAcc.Posted
                };

                _connection.Execute(query, parameters);

                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
            catch
            {
                // Handle the exception
            }

            return 0;
        }

        public int UpdateChartAcc(string AccountCode, string AccountHead)
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }

            try
            {
                string query = "UPDATE AccountLedger SET parent_name = @AccountHead WHERE code = @AccountCode";
                var parameters = new { AccountCode, AccountHead };

                _connection.Execute(query, parameters);

                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
            catch
            {
                // Handle the exception
            }

            return 0;
        }

        public List<AccountLedger> LoadChartofaccounttree(string id)
        {
            List<AccountLedger> aLoadChartofAccInfo = new List<AccountLedger>();

            try
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                string query = @"
            SELECT 
                (SELECT parent_name FROM AccountLedger WHERE id = (SELECT id FROM AccountLedger WHERE Code = @id)) AS RootAccount, 
                parent_name AS ChildAccount,
                parent_id
            FROM 
                AccountLedger 
            WHERE 
                parent_id = (SELECT id FROM AccountLedger WHERE code = @id)";

                var parameters = new { id };

                aLoadChartofAccInfo = _connection.Query<AccountLedger>(query, parameters).ToList();
            }
            catch
            {
                // Handle the exception
            }

            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }

            return aLoadChartofAccInfo;
        }
        public List<AccountLedger> LoadParentNameCode(string ParentName)
        {
            List<AccountLedger> aLoadChartofAccInfo = new List<AccountLedger>();

            try
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                string query = "SELECT Code FROM AccountLedger WHERE ParentName = @ParentName";
                var parameters = new { ParentName };

                aLoadChartofAccInfo = _connection.Query<AccountLedger>(query, parameters).ToList();
            }
            catch
            {
                // Handle the exception
            }

            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }

            return aLoadChartofAccInfo;
        }

        public List<AccountLedger> GetChartofAccList()
        {
            List<AccountLedger> aRiciveList = new List<AccountLedger>();

            try
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                string query = "SELECT code, parent_name, id, posted FROM ac_account_ledger_coa";

                aRiciveList = _connection.Query<AccountLedger>(query).ToList();
            }
            catch
            {
                // Handle the exception
            }

            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }

            return aRiciveList;
        }

        public List<AccountLedger> ViewChartofAccData()
        {
            List<AccountLedger> aRiciveList = new List<AccountLedger>();

            try
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                string query = @"
            SELECT 
                ChildUserType.Id as ParentAccount,
                ChildUserType.code as Code,
                ChildUserType.parent_name as ParentAccountName,
                ISNULL(ParentUserType.Id, 0) as ChildAccount,
                ParentUserType.parent_name as ChildAccountName
            FROM 
                ac_account_ledger_coa AS ChildUserType
            LEFT JOIN 
                ac_account_ledger_coa AS ParentUserType 
            ON 
                ChildUserType.parent_id = ParentUserType.id
            GROUP BY 
                ChildUserType.Id, ChildUserType.parent_name, ParentUserType.Id, ParentUserType.parent_name, ChildUserType.code";

                aRiciveList = _connection.Query<AccountLedger>(query).ToList();
            }
            catch
            {
                // Handle the exception
            }

            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }

            return aRiciveList;
        }

        public List<AccountLedger> ListChartofAccData()
        {
            List<AccountLedger> aRiciveList = new List<AccountLedger>();

            try
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                string query = @"
            SELECT DISTINCT
                ChildUserType.Id as ParentAccount,
                ChildUserType.code as Code,
                ChildUserType.parent_name as ParentAccountName,
                ISNULL(ParentUserType.Id, 0) as ChildAccount,
                ParentUserType.parent_name as ChildAccountName
            FROM 
                ac_account_ledger_coa AS ChildUserType
            LEFT JOIN 
                ac_account_ledger_coa AS ParentUserType 
            ON 
                ChildUserType.parent_id = ParentUserType.id
            WHERE
                ChildUserType.posted = 1
            GROUP BY 
                ChildUserType.Id, ChildUserType.parent_name, ParentUserType.Id, ParentUserType.parent_name, ChildUserType.code";

                aRiciveList = _connection.Query<AccountLedger>(query).ToList();
            }
            catch
            {
                // Handle the exception
            }

            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }

            return aRiciveList;
        }


        public AccountLedger LoadaChartofAccData(string invoice)
        {
            AccountLedger aChartofAccModel = new AccountLedger();

            try
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                string query = "SELECT code, parent_name FROM ac_account_ledger_coa WHERE code = @invoice";
                var parameters = new { invoice };

                aChartofAccModel = _connection.QueryFirstOrDefault<AccountLedger>(query, parameters);
            }
            catch
            {
                // Handle the exception
            }

            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }

            return aChartofAccModel;
        }

        public List<AccountLedger> GetLeadgrForPayVoucherSupName(int SupplierId)
        {
            List<AccountLedger> aRiciveList = new List<AccountLedger>();

            try
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                string query = @"
            SELECT DISTINCT
                ChildUserType.Id as ParentAccount,
                ChildUserType.code as Code,
                ChildUserType.parent_name as ParentAccountName,
                ISNULL(ParentUserType.Id, 0) as ChildAccount,
                ParentUserType.parent_name as ChildAccountName
            FROM 
                ac_account_ledger_coa AS ChildUserType
            LEFT JOIN 
                ac_account_ledger_coa AS ParentUserType 
            ON 
                ChildUserType.parent_id = ParentUserType.id
            WHERE
                ChildUserType.posted = 1 
                AND ChildUserType.related_id = @SupplierId
                AND ChildUserType.related_id_for = @SupplierType
            GROUP BY 
                ChildUserType.Id, ChildUserType.parent_name, ParentUserType.Id, ParentUserType.parent_name, ChildUserType.code";

                var parameters = new { SupplierId, SupplierType = 4 };

                aRiciveList = _connection.Query<AccountLedger>(query, parameters).ToList();
            }
            catch
            {
                // Handle the exception
            }

            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }

            return aRiciveList;
        }

        public List<AccountLedger> GetLeadgrForRcvVoucherSupName(int SupplierId)
        {
            List<AccountLedger> aRiciveList = new List<AccountLedger>();

            try
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                string query = @"
            SELECT DISTINCT
                ChildUserType.Id as ParentAccount,
                ChildUserType.code as Code,
                ChildUserType.parent_name as ParentAccountName,
                ISNULL(ParentUserType.Id, 0) as ChildAccount,
                ParentUserType.parent_name as ChildAccountName
            FROM 
                ac_account_ledger_coa AS ChildUserType
            LEFT JOIN 
                ac_account_ledger_coa AS ParentUserType 
            ON 
                ChildUserType.parent_id = ParentUserType.id
            WHERE
                ChildUserType.posted = 1 
                AND ChildUserType.related_id = @SupplierId
                AND ChildUserType.related_id_for = @SupplierType
            GROUP BY 
                ChildUserType.Id, ChildUserType.parent_name, ParentUserType.Id, ParentUserType.parent_name, ChildUserType.code";

                var parameters = new { SupplierId, SupplierType = 3 };

                aRiciveList = _connection.Query<AccountLedger>(query, parameters).ToList();
            }
            catch
            {
                // Handle the exception
            }

            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }

            return aRiciveList;
        }

        public List<AccountLedger> GetParentIdList(int intIdt)
        {
            List<AccountLedger> aRiciveList = new List<AccountLedger>();

            try
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                string query = "GetAllChildById";
                var parameters = new { ledgerId = intIdt };

                aRiciveList = _connection.Query<AccountLedger>(query, parameters, commandType: CommandType.StoredProcedure).ToList();
            }
            catch
            {
                // Handle the exception
            }

            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }

            return aRiciveList;
        }


        public async Task<List<AccountLedger>> GetParentLedgerByCode(string code)
        {
            List<AccountLedger> aRiciveList = new List<AccountLedger>();

            try
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                string query = "GetParentLedgerByCode";
                var parameters = new { CodeId = code };

                aRiciveList = _connection.Query<AccountLedger>(query, parameters, commandType: CommandType.StoredProcedure).ToList();
            }
            catch
            {
                // Handle the exception
            }

            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }

            return aRiciveList;
        }

        public int SaveChartOfCoa(List<AccountLedger> acclist, int intIdt)
        {
            int s = 0;

            try
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                string query = @"
            INSERT INTO ac_tb_coa(TB_AccountsLedgerCOA_id, PARENT_ID, PARENT_ID1, PARENT_ID2, PARENT_ID3, PARENT_ID4, PARENT_ID5, PARENT_ID6)
            VALUES (@TB_AccountsLedgerCOA_id, @PARENT_ID, @PARENT_ID1, @PARENT_ID2, @PARENT_ID3, @PARENT_ID4, @PARENT_ID5, @PARENT_ID6)";

                var parameters = new
                {
                    TB_AccountsLedgerCOA_id = intIdt,
                    PARENT_ID = acclist.ElementAtOrDefault(0)?.ParentId ?? 0,
                    PARENT_ID1 = acclist.ElementAtOrDefault(1)?.ParentId ?? 0,
                    PARENT_ID2 = acclist.ElementAtOrDefault(2)?.ParentId ?? 0,
                    PARENT_ID3 = acclist.ElementAtOrDefault(3)?.ParentId ?? 0,
                    PARENT_ID4 = acclist.ElementAtOrDefault(4)?.ParentId ?? 0,
                    PARENT_ID5 = acclist.ElementAtOrDefault(5)?.ParentId ?? 0,
                    PARENT_ID6 = 0
                };

                s = _connection.Execute(query, parameters);
            }
            catch
            {
                // Handle the exception
            }

            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }

            return s;
        }

        public async Task<List<AccountLedger>> GetAccountLedgerByParentId(int parentId)
        {
            List<AccountLedger> accountLedgerList = new List<AccountLedger>();

            try
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                string query = $@"SELECT 
                                child.ParentName, child.Code, 
                                RootParentName = parent.ParentName 
                                FROM AccountLedger child
                                LEFT JOIN AccountLedger parent on parent.Id = child.ParentId
                                WHERE child.ParentId =  {parentId}";
                accountLedgerList = _connection.Query<AccountLedger>(query).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
            return accountLedgerList;
        }

    }
}