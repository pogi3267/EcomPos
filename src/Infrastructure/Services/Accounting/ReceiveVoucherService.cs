using ApplicationCore.Entities;
using ApplicationCore.Entities.Accounting;
using ApplicationCore.Enums;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Accounting;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.Accounting
{
    public class ReceiveVoucherService : IReceiveVoucherService
    {
        private readonly IDapperService<AccountVoucher> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction = null;


        public ReceiveVoucherService(IDapperService<AccountVoucher> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }
        public async Task<List<AccountVoucher>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY AccountVoucherId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT 
                                    vou.AccountVoucherId
                                    ,vou.VoucherNumber
                                    ,vou.VoucherDate
                                    ,vou.AccouVoucherTypeAutoID
                                    ,br.BranchName
                                    ,vou.Narration
                                    ,SubTotal=sum(dtls.CreditAmount)
                                    ,Created_By=COALESCE(ap.FirstName,''+ap.LastName,'')
	                                , Count(*) Over() TotalRows
                               FROM AccountVoucher AS vou 
                               LEFT JOIN AccountVoucherDetails as dtls on vou.AccountVoucherId=dtls.AccountVoucherId 
                               LEFT JOIN Branch as br on br.BranchId=vou.BranchId 
                               LEFT JOIN AspNetUsers as ap on ap.Id=vou.Created_By 
                               WHERE vou.AccouVoucherTypeAutoID={(int)AccountVoucherType.RECEIEVED} AND vou.IsDeleted = 0
                               ";

                if (searchBy != "")
                    sql += " AND VoucherNumber like '%" + searchBy + "%' or VoucherDate like '%" + searchBy + "%' ";
                sql += " GROUP BY vou.AccountVoucherId, vou.VoucherNumber,vou.VoucherDate,vou.AccouVoucherTypeAutoID,vou.BranchId,vou.Narration,br.BranchName,ap.FirstName,ap.LastName";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<AccountVoucher>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<AccountVoucher> GetInitial()
        {
            try
            {
                AccountVoucher data = new AccountVoucher();
                var query = $@" 
                                SELECT Id = BranchId, Text = BranchName FROM Branch ORDER BY BranchId DESC;
                                
                                SELECT Id = OperationalUserId, Text = OP.OrganizationName, Description = OP.Address
                                FROM OperationalUser OP
                                INNER JOIN Agent AG ON OP.AgentId = AG.AgentId
                                WHERE OP.Role = 'CUSTOMER'
                                ORDER BY OperationalUserId DESC;
                                
                                select Id=AL.Id,Text='['+ParentName+']-'+ Code from AccountLedger AL;
                                ";
                var queryResult = await _connection.QueryMultipleAsync(query);
                data.BranchList = queryResult.Read<Select2OptionModel>().ToList();
                data.SupplierList = queryResult.Read<Select2OptionModel>().ToList();
                data.AccountLedgerList = queryResult.Read<Select2OptionModel>().ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<double> GetLedgerBalanceById(int accountLedgerId)
        {
            try
            {

                // some thing to do here...
                var rand = new Random();
                return await Task.FromResult(rand.NextDouble());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<Select2OptionModel>> GetAccountHeadBySupplierId(int supplierId)
        {
            try
            {
                var query = $@"select Id=AL.Id,Text='['+ParentName+']-'+ Code from AccountLedger AL Where RelatedId={supplierId}";
                var queryResult = await _connection.QueryMultipleAsync(query);
                var list = queryResult.Read<Select2OptionModel>().ToList();

                return await Task.FromResult(list);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> InvoiceGenerate()
        {
            try
            {
                await _connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@newInvoice", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                await _connection.ExecuteAsync("sp_ReciveVoucherInvoice", parameters, commandType: CommandType.StoredProcedure);
                return parameters.Get<string>("@newInvoice");
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

        private async Task<AccountVoucher> GetVoucherByVoucherIdAsync(int VoucherId, SqlTransaction transaction)
        {
            var voucher = new AccountVoucher();
            var query = $@" SELECT AccountVoucherDetailId,
                            AccountVoucherDetailId as VoucherDetailsId,AccountVoucherId,TypeId,ChildId,DebitAmount
                            ,CreditAmount,VoucherDate,Reference,IsActive,BranchId,Created_At,Updated_At
                            ,Created_By,Updated_By
                            FROM AccountVoucherDetails
                            WHERE AccountVoucherId = {VoucherId}  and TypeId={(int)AmountType.CREDIT_AMOUNT}";

            var queryResult = await _connection.QueryMultipleAsync(query, null, transaction);
            voucher.AccountVoucherDetails = queryResult.Read<AccountVoucherDetails>().ToList();

            return voucher;
        }

        public async Task<AccountVoucher> UpdateAsync(AccountVoucher entity)
        {
            try
            {
                entity.VoucherDate = DateTime.Now;
                entity.Updated_At = DateTime.Now;
                entity.EntityState = EntityState.Modified;
                await _connection.OpenAsync();
                _transaction = _connection.BeginTransaction();

                int voucherId = await _service.SaveSingleAsync(entity, _transaction);

                var previousVoucher = await GetVoucherByVoucherIdAsync(entity.AccountVoucherId, _transaction);


                for (int i = 0; i < entity.AccountVoucherDetails.Count; i++)
                {
                    var currentVoucher = entity.AccountVoucherDetails[i];
                    var voucherDetailExist = previousVoucher.AccountVoucherDetails
                                            .Find(prevDetail => prevDetail.VoucherDetailsId == currentVoucher.AccountVoucherDetailId);
                    if (voucherDetailExist != null)
                    {
                        voucherDetailExist.EntityState = EntityState.Modified;
                        voucherDetailExist.Updated_At = DateTime.Now;
                        voucherDetailExist.Updated_By = entity.Updated_By;
                        entity.AccountVoucherDetails[i] = voucherDetailExist;
                    }
                    else
                    {
                        currentVoucher.EntityState = EntityState.Added;
                        currentVoucher.AccountVoucherId = entity.AccountVoucherId;
                        currentVoucher.Created_At = DateTime.Now;
                        currentVoucher.Created_By = entity.Updated_By;
                        currentVoucher.VoucherDate = DateTime.Now;
                        currentVoucher.TypeId = AmountType.DEBIT_AMOUNT;
                        currentVoucher.BranchId = entity.BranchId;
                        entity.AccountVoucherDetails[i] = currentVoucher;
                    }
                }

                foreach (var vouDet in previousVoucher.AccountVoucherDetails)
                {
                    var currentDetailExist = entity.AccountVoucherDetails.Find(curDet => curDet.AccountVoucherDetailId == vouDet.VoucherDetailsId);
                    if (currentDetailExist == null)
                    {
                        AccountVoucherDetails voucherDetailToDelete = vouDet;
                        voucherDetailToDelete.EntityState = EntityState.Deleted;
                        entity.AccountVoucherDetails.Add(voucherDetailToDelete);
                    }
                };
                var sqlQuery = $@"UPDATE AccountVoucherDetails SET ChildId = @childId, CreditAmount = @creditAmount, Updated_At = GETDATE(), Updated_By = @updatedBy
                                    WHERE AccountVoucherId = @accountVoucherId AND TypeId = 2;";
                await _service.ExecuteQueryAsync(sqlQuery, new
                {
                    childId = entity.AccountLedgerId,
                    creditAmount = (decimal)entity.SubTotal,
                    updatedBy = entity.Updated_By,
                    accountVoucherId = entity.AccountVoucherId
                }
                                                , _transaction);
                await _service.SaveAsync<AccountVoucherDetails>(entity.AccountVoucherDetails, _transaction);

                _transaction.Commit();
                return entity;
            }
            catch (Exception ex)
            {
                _transaction.Rollback();
                throw;
            }
        }

        public async Task<int> SaveAsync(AccountVoucher entity)
        {
            try
            {

                await _connection.OpenAsync();
                _transaction = _connection.BeginTransaction();

                var id = await _service.SaveSingleAsync(entity, _transaction);

                entity.AccountVoucherDetails.Insert(0, new AccountVoucherDetails
                {
                    AccountVoucherId = id,
                    ChildId = entity.AccountLedgerId,
                    DebitAmount = (decimal)entity.SubTotal,
                    TypeId = AmountType.DEBIT_AMOUNT,
                    IsActive = true,
                    VoucherDate = DateTime.Now,
                    BranchId = entity.BranchId,
                    Created_At = DateTime.Now,
                    Created_By = entity.Created_By,
                });
                entity.AccountVoucherDetails.ForEach(item =>
                {
                    item.AccountVoucherId = id;
                    if (item.TypeId != AmountType.DEBIT_AMOUNT)
                    {
                        item.TypeId = AmountType.CREDIT_AMOUNT;
                    }
                    item.IsActive = true;
                    item.VoucherDate = DateTime.Now;
                    item.BranchId = entity.BranchId;
                    item.Created_At = DateTime.Now;
                    item.Created_By = entity.Created_By;
                });

                await _service.SaveAsync<AccountVoucherDetails>(entity.AccountVoucherDetails, _transaction);

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

        public async Task<AccountVoucher> GetAsync(int id)
        {
            try
            {
                var query = $@"SELECT * FROM AccountVoucher WHERE AccountVoucherId = {id};
                               
                               SELECT AccountVoucherDetailId as VoucherDetailsId,AccountVoucherId,TypeId,ChildId,DebitAmount
                              ,CreditAmount,VoucherDate,Reference,IsActive,BranchId,Created_At,Updated_At
                              ,Created_By,Updated_By FROM AccountVoucherDetails WHERE AccountVoucherId = {id}  and TypeId={(int)AmountType.CREDIT_AMOUNT};

                               SELECT Id = BranchId, Text = BranchName FROM Branch ORDER BY BranchId DESC;
                               
                               SELECT Id = OperationalUserId, Text = AG.AgentName +' | ' + OP.Code, Description = OP.Address
                               FROM OperationalUser OP
                               INNER JOIN Agent AG ON OP.AgentId = AG.AgentId
                               WHERE OP.Role = 'CUSTOMER'
                               ORDER BY OperationalUserId DESC;
                               
                               SELECT Id, ParentName + '|' +  Code as Text FROM AccountLedger ORDER BY Id DESC;";


                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                AccountVoucher data = queryResult.Read<AccountVoucher>().FirstOrDefault();
                data.AccountVoucherDetails = queryResult.Read<AccountVoucherDetails>().ToList();
                data.BranchList = queryResult.Read<Select2OptionModel>().ToList();
                data.SupplierList = queryResult.Read<Select2OptionModel>().ToList();
                data.AccountLedgerList = queryResult.Read<Select2OptionModel>().ToList();

                data.SupplierId = data.CustomerId;
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

        public async Task<int> DeleteAsync(int accountVoucherId, string userId)
        {
            try
            {
                await _connection.OpenAsync();
                _transaction = _connection.BeginTransaction();
                var query = $@"
                                UPDATE AccountVoucher SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @userId WHERE AccountVoucherId = @accountVoucherId;
                                UPDATE AccountVoucherDetails SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @userId WHERE AccountVoucherId = @accountVoucherId;
                              ";
                await _service.ExecuteQueryAsync(query, new { userId, accountVoucherId }, _transaction);
                _transaction.Commit();
                return accountVoucherId;
            }
            catch (Exception ex)
            {
                _transaction.Rollback();
                throw ex;
            }
        }
    }
}