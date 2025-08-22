using ApplicationCore.Entities;
using ApplicationCore.Entities.Inventory;
using ApplicationCore.Utilities;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Inventory;
using Microsoft.Data.SqlClient;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.Inventory
{
    public class CollectionDepositService : ICollectionDepositService
    {
        private readonly IDapperService<Collection> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction = null;

        public CollectionDepositService(IDapperService<Collection> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<Collection> GetInitial()
        {
            Collection data = new Collection();
            var query = $@" 
                            SELECT Id = Id, Text = ParentName FROM AccountLedger WHERE ParentId=59 ORDER BY Id DESC
                            SELECT Id = Id, Text = Code FROM AccountLedger WHERE ParentId=59 ORDER BY Id DESC;
                            ";
            var queryResult = await _connection.QueryMultipleAsync(query);
            data.DepositBankList = queryResult.Read<Select2OptionModel>().ToList();
            data.DepositAccountNumberList = queryResult.Read<Select2OptionModel>().ToList();
            return data;
        }
        public async Task<List<Collection>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            //CAST(CL.Created_At AS date) = CAST(GETDATE() AS date)
            string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY CollectionId DESC" : "ORDER BY " + sortBy + " " + sortDir;
            string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
            string sql = $@"SELECT CL.*, CustomerName = CUS.Name, BN.BankName,  Count(*) Over() TotalRows FROM Collection CL
                            LEFT JOIN Bank BN ON BN.BankId = CL.BankId
                            LEFT JOIN OperationalUser CUS ON CUS.OperationalUserId = CL.CustomerId AND CUS.Role = '{OperationalUserRole.Customer}'
                            WHERE CL.DepositStatus!='Y' 
                            ";
            if (searchBy != "")
                sql += " AND InvoiceNoCollection like '%" + searchBy + "%' ";
            sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
            var result = await _service.GetDataAsync<Collection>(sql);
            return result;
        }

        public async Task<Collection> GetAsync(int id)
        {
            try
            {
                Collection data = new Collection();
                var query = $@" 
                            SELECT CL.*, CustomerName = CUS.Name, CustomerCode = CUS.Code, BN.BankName FROM Collection CL
                            LEFT JOIN Bank BN ON BN.BankId = CL.BankId
                            LEFT JOIN OperationalUser CUS ON CUS.OperationalUserId = CL.CustomerId AND CUS.Role = '{OperationalUserRole.Customer}'
                            WHERE CL.CollectionId = {id};

                            SELECT Id = BankId, Text = BankName FROM Bank ORDER BY BankId DESC;
                            ";
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                data = queryResult.Read<Collection>().FirstOrDefault();
                data.BankList = queryResult.Read<Select2OptionModel>().ToList();
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

        public async Task<int> SaveAsync(Collection entity)
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
    }
}
