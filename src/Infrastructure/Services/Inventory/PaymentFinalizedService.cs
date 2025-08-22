using ApplicationCore.Common;
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
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.Inventory
{
    public class PaymentFinalizedService : IPaymentFinalizedService
    {
        private readonly IDapperService<Payment> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction = null;

        public PaymentFinalizedService(IDapperService<Payment> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

       
        public async Task<List<Payment>>  GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            //CAST(CL.Created_At AS date) = CAST(GETDATE() AS date)
            string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY PaymentId DESC" : "ORDER BY " + sortBy + " " + sortDir;
            string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
            string sql = $@"SELECT PA.*, SupplierName = CUS.Name, BankName=ABN.ParentName,  Count(*) Over() TotalRows FROM Payment PA
                            LEFT JOIN AccountLedger ABN ON ABN.Id = PA.BankId
                            LEFT JOIN OperationalUser CUS ON CUS.OperationalUserId = PA.SupplierId AND CUS.Role = 'SUPPLIER'
                            WHERE PA.Approve =3  
                            ";
            if (searchBy != "")
                sql += " AND InvoiceNoPayment like '%" + searchBy + "%' ";
            sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
            var result = await _service.GetDataAsync<Payment>(sql);
            return result;
        }

        public async Task<Payment> GetAsync(int id)
        {
            try
            {
                Payment data = new Payment();
                var query = $@" 
                            SELECT PA.*, SupplierName = CUS.Name, SupplierCode = CUS.Code, BankName=ABN.ParentName FROM Payment PA
                            LEFT JOIN AccountLedger ABN ON ABN.Id = PA.BankId
                            LEFT JOIN OperationalUser CUS ON CUS.OperationalUserId = PA.SupplierId AND CUS.Role = 'SUPPLIER'
                            WHERE PA.PaymentId = {id};
                            ";
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                data = queryResult.Read<Payment>().FirstOrDefault();
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

        public async Task<int> SaveAsync(Payment entity)
        {
            try
            {
                int ledgerId = await GetLedgerIdByOperationalUser(entity.SupplierId);

                await _connection.OpenAsync();
                _transaction = _connection.BeginTransaction();
                var id = await _service.SaveSingleAsync(entity, _transaction);

                if(entity.Approve == 1)
                {
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

        public async Task SaveVoucherAsync(Payment entity, int ledgerId, SqlTransaction transaction)
        {
            AccountVoucher accountVoucher = new AccountVoucher()
            {
                AccouVoucherTypeAutoID = (int)AccountVoucherType.PAYMENT,
                VoucherNumber = entity.PayInvoiceNo,
                VoucherDate = DateTime.Now,
                BranchId = entity.BranchId,
                SupplierId = entity.SupplierId,
                IsActive = true,
                AccountType = 2, //supplier
                AccountLedgerId = 34, // Supplier Payment
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
                CreditAmount = (decimal)entity.PayAmount,
                TypeId = AmountType.CREDIT_AMOUNT,
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
                DebitAmount = (decimal)entity.PayAmount,
                TypeId = AmountType.DEBIT_AMOUNT,
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
            var query = $@"SELECT Id FROM AccountLedger WHERE RelatedId = {id}";
            return await _service.GetSingleIntFieldAsync(query);
        }

        public async Task DeleteVoucher(string voucherNumber, SqlTransaction transaction)
        {
            var query = $@"
                DELETE FROM AccountVoucherDetails WHERE AccountVoucherId = (SELECT AccountVoucherId FROM AccountVoucher WHERE VoucherNumber = (SELECT PayInvoiceNo FROM Payment WHERE PayInvoiceNo = @voucherNumber));
                DELETE FROM AccountVoucher WHERE VoucherNumber = (SELECT PayInvoiceNo FROM Payment WHERE PayInvoiceNo = @voucherNumber)
                ";
            await _service.ExecuteQueryAsync(query, new { voucherNumber }, transaction);
        }

    }
}
