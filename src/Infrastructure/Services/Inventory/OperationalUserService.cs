using ApplicationCore.Entities;
using ApplicationCore.Entities.Inventory;
using ApplicationCore.Enums;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Inventory;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.Inventory
{
    public class OperationalUserService : IOperationalUserService
    {
        private readonly IDapperService<OperationalUser> _service;
        //private readonly IAccountLedgerService _accountLedgerService;
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction = null;

        public OperationalUserService(IDapperService<OperationalUser> service) : base()
        {
            _service = service;
            //_accountLedgerService = accountLedgerService;
            _connection = service.Connection;
        }

        public async Task<OperationalUser> GetInitial()
        {
            OperationalUser data = new OperationalUser();
           
            return data;
        }
        public async Task<List<OperationalUser>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir, string roleName = null)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY OperationalUserId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT *, Count(*) Over() TotalRows FROM OperationalUser";
                sql += $@" WHERE Role = '{roleName}'";
                if (searchBy != "")
                    sql += " AND OrganizationName like '%" + searchBy + "%' or Code like '%" + searchBy + "%' ";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<OperationalUser>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<OperationalUser> GetAsync(int id)
        {
            try
            {
                var query = $@"SELECT * FROM OperationalUser WHERE OperationalUserId = {id};";
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                OperationalUser data = queryResult.Read<OperationalUser>().FirstOrDefault();
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

        public async Task<string> GetAutoGenerateCodeByRoleAsync(string roleName, SqlTransaction _sqlTransaction)
        {
            string query = $@"SELECT TOP 1 * FROM OperationalUser WHERE Role = '{roleName}' ORDER BY OperationalUserId DESC;";
            var data = await _connection.QueryFirstOrDefaultAsync<OperationalUser>(query, null, _sqlTransaction);
            int lastNumber = 1;
            if (data?.Code != null)
            {
                lastNumber = Convert.ToInt32(data.Code.Split('-').Last()) + 1;
            }
            return $@"{roleName.Substring(0, 3)}-{lastNumber}";
        }



        public async Task<int> SaveAsync(OperationalUser entity)
        {
            try
            {
                await _connection.OpenAsync();
                _transaction = _connection.BeginTransaction();

                string userId = entity.OperationalUserId > 0 ? entity.Updated_By : entity.Created_By;

                if (entity.EntityState == EntityState.Added)
                    entity.Code = await GetAutoGenerateCodeByRoleAsync(entity.Role, _transaction);
                var operationalUserId = await _service.SaveSingleAsync(entity, _transaction);

                //#region account ledger creation

                //string supplierOrCustomerCode = string.Empty;
                //var accountLedger = new AccountLedger()
                //{
                //    ParentAccountName = supplierOrCustomerCode,
                //    RelatedIdFor = 4,
                //    RelatedId = operationalUserId,
                //    Posted = true,
                //    Created_At = DateTime.Now,
                //    Created_By = userId,
                //};

                //if (entity.Role == AutoRiceMillRole.Supplier)
                //{
                //    supplierOrCustomerCode = "2-001-001";
                //    accountLedger.ParentName = entity.Name;
                //    accountLedger.ParentId = 34; //Supplier Payment ACC Ledger Id
                //}
                //else
                //{
                //    supplierOrCustomerCode = "1-001-002";
                //    accountLedger.ParentName = entity.Name;
                //    accountLedger.ParentId = 21;
                //}
                //if (entity.EntityState == EntityState.Added)
                //{
                //    string accountLedgerCode = await _accountLedgerService.CodeGenarator(supplierOrCustomerCode);
                //    accountLedger.Code = accountLedgerCode;
                //    var accountLedgerId = await _accountLedgerService.SaveAsync(accountLedger);
                //    List<AccountLedger> allChildList = _accountLedgerService.GetParentIdList(accountLedgerId);
                //    _accountLedgerService.SaveChartOfCoa(allChildList, accountLedgerId);
                //}
                //#endregion

                _transaction.Commit();
                return operationalUserId;
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw ex;
            }
            finally
            {
                _transaction.Dispose();
                _connection.Close();
            }
        }

        public async Task<List<OperationalUser>> GetOperationalUserByRole(string roleName)
        {
            var query = $@"SELECT * FROM OperationalUser WHERE Role = '{roleName}'";
            var result = await _service.GetDataAsync(query);
            return result.ToList();
        }
    }
}
