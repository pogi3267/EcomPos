using ApplicationCore.Common;
using ApplicationCore.DTOs;
using ApplicationCore.Entities;
using ApplicationCore.Entities.Accounting;
using ApplicationCore.Entities.Inventory;
using ApplicationCore.Enums;
using ApplicationCore.Utilities;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Inventory;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Infrastructure.Services.Inventory
{
    public class CollectionService : ICollectionService
    {
        private readonly IDapperService<Collection> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction = null;

        public CollectionService(IDapperService<Collection> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<Collection> GetInitial()
        {
            try
            {
                Collection data = new Collection();
                var query = $@" 
                                SELECT Id = OperationalUserId, Text = COALESCE(Name, '') + ' |' + COALESCE(Code, '') 
                                FROM OperationalUser OP
                                INNER JOIN Salses S ON S.CustomerId = OP.OperationalUserId
                                WHERE OP.Role = 'CUSTOMER' GROUP BY OperationalUserId,Name,Code
                                ORDER BY OperationalUserId DESC;
                                
                                SELECT Id = BankId, Text = BankName FROM Bank ORDER BY BankId DESC;
                                
                                SELECT Id = BranchId, Text = BranchName FROM Branch ORDER BY BranchId DESC
                                
                                select * from Salses;
                                ";
                var queryResult = await _connection.QueryMultipleAsync(query);
                data.CustomerList = queryResult.Read<Select2OptionModel>().ToList();
                data.BankList = queryResult.Read<Select2OptionModel>().ToList();
                data.BranchList = queryResult.Read<Select2OptionModel>().ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<dynamic> GetCustomerDueAmount(int customerId)
        {
            try
            {
                if (customerId == 5001) //retail customer
                {
                    string query = $@"EXEC sp_CalculateBalanceByRetailCustomer {customerId}";
                    var data = await _service.GetDynamicDataAsync(query);

                    var dt = data.Select(item => new
                    {
                        Invoice = item.Invoice,
                        SalseAmount = item.SalseAmount,
                        CollAmount = item.CollAmount
                    }).FirstOrDefault();

                    //return Convert.ToDouble(dt.SalseAmount);
                    return dt;
                }
                else
                {
                    string query = $@"EXEC sp_CalculateOldBalanceByCustomer {customerId}";
                    var data = await _service.GetDynamicDataAsync(query);

                    var dt = data.Select(item => new
                    {
                        oldbalance = item.oldbalance,
                        depositBalance = item.depositBalance
                    }).FirstOrDefault();

                    //return Convert.ToDouble(dt.oldbalance);
                    return dt;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public async Task<List<Collection>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY CollectionId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT CL.*, CustomerName = CUS.Name, BN.BankName,  Count(*) Over() TotalRows FROM Collection CL
                                LEFT JOIN Bank BN ON BN.BankId = CL.BankId
                                LEFT JOIN OperationalUser CUS ON CUS.OperationalUserId = CL.CustomerId AND CUS.Role = '{OperationalUserRole.Customer}'
                                WHERE CAST(CL.Created_At AS date) = CAST(GETDATE() AS date) 
                                ";
                if (searchBy != "")
                    sql += " AND InvoiceNoCollection like '%" + searchBy + "%' ";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<Collection>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Collection> GetAsync(int id)
        {
            try
            {
                var query = $@"SELECT * FROM Collection WHERE CollectionId = {id};";
                await _connection.OpenAsync();
                Collection data = await _connection.QuerySingleAsync<Collection>(query);
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
                int ledgerId = await GetLedgerIdByOperationalUser(entity.CustomerId);

                await _connection.OpenAsync();
                _transaction = _connection.BeginTransaction();
                var id = await _service.SaveSingleAsync(entity, _transaction);

                if (entity.Approved)
                {
                    if (entity.EntityState == EntityState.Modified)
                        await DeleteVoucher(entity.InvoiceNoCollection, _transaction);

                    await SaveVoucherAsync(entity, ledgerId, _transaction);
                }

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

        public async Task<string> InvoiceGenerate()
        {
            try
            {
                var query = $@"SELECT Top 1 *
                                FROM Collection
                                ORDER BY CollectionId DESC;";
                var queryResult = await _connection.QueryMultipleAsync(query);
                var purchase = queryResult.Read<Collection>().FirstOrDefault();
                return purchase != null ? ProcessedInvoiceNumber(purchase.InvoiceNoCollection) : ProcessedInvoiceNumber("0");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string ProcessedInvoiceNumber(string invoiceNumber)
        {
            string prefix = "COL-";
            string newInvoiceNumber = prefix;
            int invoiceLength = 6;

            invoiceNumber = invoiceNumber == "0" ? prefix + invoiceNumber : invoiceNumber;
            string nextNumber = Convert.ToString(Convert.ToInt32(invoiceNumber.Split("-")[1]) + 1);
            for (int i = 0; i < invoiceLength - nextNumber.Length; i++)
            {
                newInvoiceNumber += "0";
            }
            return newInvoiceNumber + nextNumber;
        }

        public async Task<OperationalUser> GetCustomerById(int customerId)
        {
            try
            {
                OperationalUser data = new OperationalUser();
                var query = $@" select * from OperationalUser WHERE OperationalUserId = {customerId}; ";
                var queryResult = await _connection.QueryAsync<OperationalUser>(query);
                return queryResult.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<RepDateWiseCollectionDTO>> CollectionReportDateWise(CollectionReport collectionReport)
        {
            try
            {
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@dateFrom", Convert.ToDateTime(collectionReport.FromDate).ToString("MM-dd-yyyy"));
                queryParameters.Add("@dateTo", Convert.ToDateTime(collectionReport.ToDate).ToString("MM-dd-yyyy"));
                queryParameters.Add("@collectiontype", collectionReport.CollectionType);
                queryParameters.Add("@customarId", collectionReport.Customer);
                var data = await _connection.QueryAsync<RepDateWiseCollectionDTO>("Rep_Collection",
                             queryParameters, commandType: CommandType.StoredProcedure);
                return data.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<RepInvoiceCollectionDTO>> CollectionReportInvoiceWise(CollectionReport collectionReport)
        {
            try
            {
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@invoiceNoCollection", collectionReport.InvoiceId);
                var data = await _connection.QueryAsync<RepInvoiceCollectionDTO>("Rep_InvoiceWiseCollection",
                             queryParameters, commandType: CommandType.StoredProcedure);
                return data.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SaveVoucherAsync(Collection entity, int ledgerId, SqlTransaction transaction)
        {
            AccountVoucher accountVoucher = new AccountVoucher()
            {
                AccouVoucherTypeAutoID = (int)AccountVoucherType.RECEIEVED,
                VoucherNumber = entity.InvoiceNoCollection,
                VoucherDate = DateTime.Now,
                BranchId = entity.BranchId,
                CustomerId = entity.CustomerId,
                IsActive = true,
                AccountType = 3, //Customer
                AccountLedgerId = 7, // Current Asset
                Created_At = DateTime.Now,
                Created_By = entity.Created_By,
                EntityState = EntityState.Added,
                IpAddress = Common.GetIpAddress()
            };

            var accountVoucherId = await _service.SaveSingleAsync<AccountVoucher>(accountVoucher, transaction);

            accountVoucher.AccountVoucherDetails.Add(new AccountVoucherDetails
            {
                AccountVoucherId = accountVoucherId,
                ChildId = accountVoucher.AccountLedgerId,
                CreditAmount = (decimal)entity.CollectionAmount,
                TypeId = AmountType.DEBIT_AMOUNT,
                IsActive = true,
                VoucherDate = DateTime.Now,
                BranchId = entity.BranchId,
                Created_At = DateTime.Now,
                Created_By = entity.Created_By,
                EntityState = EntityState.Added
            });

            accountVoucher.AccountVoucherDetails.Add(new AccountVoucherDetails
            {
                AccountVoucherId = accountVoucherId,
                ChildId = ledgerId,
                DebitAmount = (decimal)entity.CollectionAmount,
                TypeId = AmountType.CREDIT_AMOUNT,
                IsActive = true,
                VoucherDate = DateTime.Now,
                BranchId = entity.BranchId,
                Created_At = DateTime.Now,
                Created_By = entity.Created_By,
                EntityState = EntityState.Added
            });

            await _service.SaveAsync<AccountVoucherDetails>(accountVoucher.AccountVoucherDetails, transaction);
        }

        public async Task<int> GetLedgerIdByOperationalUser(int id)
        {
            var query = $@"SELECT ISNULL(Id, 0) Id FROM AccountLedger WHERE RelatedId = {id}";
            return await _service.GetSingleIntFieldAsync(query);
        }

        public async Task DeleteVoucher(string voucherNumber, SqlTransaction transaction)
        {
            var query = $@"
                DELETE FROM AccountVoucherDetails WHERE AccountVoucherId = (SELECT AccountVoucherId FROM AccountVoucher WHERE VoucherNumber = (SELECT InvoiceNoCollection FROM Collection WHERE InvoiceNoCollection = @voucherNumber));
                DELETE FROM AccountVoucher WHERE VoucherNumber = (SELECT InvoiceNoCollection FROM Collection WHERE InvoiceNoCollection = @voucherNumber)
                ";
            await _service.ExecuteQueryAsync(query, new { voucherNumber }, transaction);
        }

    }
}
