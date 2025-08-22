using ApplicationCore.Entities.Accounting;
using Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Accounting
{
    public class CommonVoucherService
    {
        private readonly IDapperService<AccountVoucher> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction = null;


        public CommonVoucherService(IDapperService<AccountVoucher> service) : base()
        {
            _service = service;
            //_connection = service.Connection;
        }
        public async Task<int> GetLedgerIdByOperationalUser(int id)
        {
            var query = $@"SELECT Id FROM AccountLedger WHERE RelatedId = {id}";
            return await _service.GetSingleIntFieldAsync(query);
        }

        public async Task SaveVoucherAsync(AccountVoucher accountVoucher, SqlTransaction transaction)
        {
            var accountVoucherId = await _service.SaveSingleAsync<AccountVoucher>(accountVoucher, transaction);
            accountVoucher.AccountVoucherDetails.ForEach(v =>
            {
                v.AccountVoucherId = accountVoucherId;
            });
            await _service.SaveAsync<AccountVoucherDetails>(accountVoucher.AccountVoucherDetails, transaction);
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
