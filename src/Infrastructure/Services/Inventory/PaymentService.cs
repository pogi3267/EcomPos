using ApplicationCore.Common;
using ApplicationCore.Entities;
using ApplicationCore.Entities.Accounting;
using ApplicationCore.Entities.Inventory;
using ApplicationCore.Enums;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Inventory;
using Infrastructure.Services.Accounting;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Infrastructure.Services.Inventory
{
    public class PaymentService : IPaymentService
    {
        private readonly IDapperService<Payment> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction = null;
        private CommonVoucherService _commonVoucherService;

        public PaymentService(IDapperService<Payment> service, IDapperService<AccountVoucher> serviceVoucher,
            CommonVoucherService commonVoucherService) : base()
        {
            _service = service;
            _connection = service.Connection;
            _commonVoucherService = commonVoucherService;
        }

        public async Task<Payment> GetInitial()
        {
            try
            {
                Payment data = new Payment();
                var query = $@" 
                                SELECT Id = OperationalUserId, Text = COALESCE(Name, '') + ' |' + COALESCE(Code, '') 
                                FROM OperationalUser OP
                                WHERE OP.Role = 'SUPPLIER'
                                ORDER BY OperationalUserId DESC;

                                SELECT Id = Id, Text = ParentName FROM AccountLedger WHERE ParentId=59 ORDER BY Id DESC

                                SELECT Id = Id, Text = Code FROM AccountLedger WHERE ParentId=59 ORDER BY Id DESC;

                                SELECT Id = BranchId, Text = BranchName FROM Branch ORDER BY BranchId DESC;
                                ";
                var queryResult = await _connection.QueryMultipleAsync(query);
                data.SupplierList = queryResult.Read<Select2OptionModel>().ToList();
                data.BankList = queryResult.Read<Select2OptionModel>().ToList();
                data.BankAccountList = queryResult.Read<Select2OptionModel>().ToList();
                data.BranchList = queryResult.Read<Select2OptionModel>().ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<dynamic> GetSupplierDueAmount(int SupplierId)
        {
            try
            {
                string query = $@"EXEC SP_CalculateSupplierDue {SupplierId}";
                var data = await _service.GetDynamicDataAsync(query);

                var dt = data.Select(item => new
                {
                    dueBalance = item.dueBalance
                }).FirstOrDefault();

                return dt;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public async Task<List<Payment>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY PaymentId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT CL.*, SupplierName = CUS.Name, BankName=ABN.ParentName,  Count(*) Over() TotalRows FROM Payment CL
                                 LEFT JOIN AccountLedger ABN ON ABN.Id = CL.BankId
                                LEFT JOIN OperationalUser CUS ON CUS.OperationalUserId = CL.SupplierId AND CUS.Role = 'SUPPLIER'
                                WHERE CAST(CL.Created_At AS date) = CAST(GETDATE() AS date) 
                                ";
                if (searchBy != "")
                    sql += " AND InvoiceNoPayment like '%" + searchBy + "%' ";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<Payment>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Payment> GetAsync(int id)
        {
            try
            {
                var query = $@"SELECT * FROM Payment WHERE PaymentId = {id};";
                await _connection.OpenAsync();
                Payment data = await _connection.QuerySingleAsync<Payment>(query);
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

                if (entity.Approve == 1)
                {
                    if (entity.EntityState == EntityState.Modified)
                        await DeleteVoucher(entity.PayInvoiceNo, _transaction);

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
                                FROM Payment
                                ORDER BY PaymentId DESC;";
                var queryResult = await _connection.QueryMultipleAsync(query);
                var purchase = queryResult.Read<Payment>().FirstOrDefault();
                return purchase != null ? ProcessedInvoiceNumber(purchase.PayInvoiceNo) : ProcessedInvoiceNumber("0");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string ProcessedInvoiceNumber(string invoiceNumber)
        {
            string prefix = "PAY-";
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

        public async Task<OperationalUser> GetSupplierById(int SupplierId)
        {
            try
            {
                OperationalUser data = new OperationalUser();
                var query = $@" select * from OperationalUser WHERE OperationalUserId = {SupplierId}; ";
                var queryResult = await _connection.QueryAsync<OperationalUser>(query);
                return queryResult.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
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