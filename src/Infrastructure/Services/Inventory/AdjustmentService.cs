using ApplicationCore.Common;
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
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using static Dapper.SqlMapper;

namespace Infrastructure.Services.Inventory
{
    public class AdjustmentService : IAdjustmentService
    {
        private readonly IDapperService<Adjustment> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction = null;

        public AdjustmentService(IDapperService<Adjustment> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }
        public async Task<List<Adjustment>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY AdjustmentId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT ADJ.*, OperationalUserName = OP.Name  FROM Adjustment ADJ
                                LEFT JOIN OperationalUser OP on OP.OperationalUserId = ADJ.OperationalUserId
                                ";
                if (searchBy != "")
                    sql += " WHERE InvoiceNumber like '%" + searchBy + "%' ";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<Adjustment>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Adjustment> GetAsync(int id)
        {
            try
            {
                var query = $@"SELECT * FROM Adjustment WHERE AdjustmentId = {id};";
                await _connection.OpenAsync();
                Adjustment data = await _connection.QuerySingleAsync<Adjustment>(query);
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

        public async Task<int> SaveAsync(Adjustment entity)
        {
            try
            {
                int ledgerId = await GetLedgerIdByOperationalUser(entity.OperationalUserId);

                if (entity.AdjustmentId == 0)
                    entity.InvoiceNumber = await InvoiceGenerate();
                await _connection.OpenAsync();
                _transaction = _connection.BeginTransaction();
                var id = await _service.SaveSingleAsync(entity, _transaction);

                if (entity.EntityState == EntityState.Modified)
                    await DeleteVoucher(entity.InvoiceNumber, _transaction);
                await SaveVoucherAsync(entity, ledgerId, _transaction);

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
                                FROM Adjustment
                                ORDER BY AdjustmentId DESC;";
                var queryResult = await _connection.QueryMultipleAsync(query);
                var purchase = queryResult.Read<Adjustment>().FirstOrDefault();
                return purchase != null ? ProcessedInvoiceNumber(purchase.InvoiceNumber) : ProcessedInvoiceNumber("0");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string ProcessedInvoiceNumber(string invoiceNumber)
        {
            string prefix = "ADJ-";
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

        public async Task SaveVoucherAsync(Adjustment entity, int ledgerId, SqlTransaction transaction)
        {
            if(entity.AdjustmentFor.ToUpper() == "SUPPLIER")
            {
                AccountVoucher accountVoucher = new AccountVoucher()
                {
                    AccouVoucherTypeAutoID = (int)AccountVoucherType.PAYMENT,
                    VoucherNumber = entity.InvoiceNumber,
                    VoucherDate = DateTime.Now,
                    BranchId = 0,
                    SupplierId = entity.OperationalUserId,
                    IsActive = true,
                    AccountType = 2, //supplier
                    AccountLedgerId = 2075, // Adjustment
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
                    CreditAmount = (decimal)entity.Amount,
                    TypeId = AmountType.CREDIT_AMOUNT,
                    IsActive = true,
                    VoucherDate = DateTime.Now,
                    BranchId = 0,
                    Created_At = DateTime.Now,
                    Created_By = entity.Created_By,
                    EntityState = EntityState.Added
                });

                accountVoucher.AccountVoucherDetails.Add(new AccountVoucherDetails
                {
                    AccountVoucherId = accountVoucherId,
                    ChildId = ledgerId,
                    DebitAmount = (decimal)entity.Amount,
                    TypeId = AmountType.DEBIT_AMOUNT,
                    IsActive = true,
                    VoucherDate = DateTime.Now,
                    BranchId = 0,
                    Created_At = DateTime.Now,
                    Created_By = entity.Created_By,
                    EntityState = EntityState.Added
                });

                await _service.SaveAsync<AccountVoucherDetails>(accountVoucher.AccountVoucherDetails, transaction);
            }
            else //CUSTOMER
            {
                AccountVoucher accountVoucher = new AccountVoucher()
                {
                    AccouVoucherTypeAutoID = (int)AccountVoucherType.RECEIEVED,
                    VoucherNumber = entity.InvoiceNumber,
                    VoucherDate = DateTime.Now,
                    BranchId = 0,
                    SupplierId = entity.OperationalUserId,
                    IsActive = true,
                    AccountType = 3, //Customer
                    AccountLedgerId = 2075, // Adjustment
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
                    CreditAmount = (decimal)entity.Amount,
                    TypeId = AmountType.DEBIT_AMOUNT,
                    IsActive = true,
                    VoucherDate = DateTime.Now,
                    BranchId = 0,
                    Created_At = DateTime.Now,
                    Created_By = entity.Created_By,
                    EntityState = EntityState.Added
                });

                accountVoucher.AccountVoucherDetails.Add(new AccountVoucherDetails
                {
                    AccountVoucherId = accountVoucherId,
                    ChildId = ledgerId,
                    DebitAmount = (decimal)entity.Amount,
                    TypeId = AmountType.CREDIT_AMOUNT,
                    IsActive = true,
                    VoucherDate = DateTime.Now,
                    BranchId = 0,
                    Created_At = DateTime.Now,
                    Created_By = entity.Created_By,
                    EntityState = EntityState.Added
                });

                await _service.SaveAsync<AccountVoucherDetails>(accountVoucher.AccountVoucherDetails, transaction);
            }
        }

        public async Task<int> GetLedgerIdByOperationalUser(int id)
        {
            var query = $@"SELECT Id FROM AccountLedger WHERE RelatedId = {id}";
            return await _service.GetSingleIntFieldAsync(query);
        }

        public async Task DeleteVoucher(string voucherNumber, SqlTransaction transaction)
        {
            var query = $@"
                DELETE FROM AccountVoucherDetails WHERE AccountVoucherId = (SELECT AccountVoucherId FROM AccountVoucher WHERE VoucherNumber = (SELECT InvoiceNumber FROM Adjustment WHERE InvoiceNumber = @voucherNumber));
                DELETE FROM AccountVoucher WHERE VoucherNumber = (SELECT InvoiceNumber FROM Adjustment WHERE InvoiceNumber = @voucherNumber)
                ";
            await _service.ExecuteQueryAsync(query, new { voucherNumber }, transaction);
        }
    }
}