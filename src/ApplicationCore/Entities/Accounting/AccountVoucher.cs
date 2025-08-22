using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.Accounting
{
    [Table("AccountVoucher")]
    public class AccountVoucher : BaseEntity
    {
        public AccountVoucher()
        {
            IsActive = true;
            VoucherDate= DateTime.Now;
            AccountVoucherDetails = new List<AccountVoucherDetails>();
        }

        [Key]
        public int AccountVoucherId { get; set; }
        public int? AccouVoucherTypeAutoID { get; set; }
        public string VoucherNumber { get; set; }
        public DateTime VoucherDate { get; set; }
        public string ReferenceInvoiceNumber { get; set; }
        public string Narration { get; set; }
        public int? CompanyId { get; set; }
        public int? BranchId { get; set; }
        public string Reference { get; set; }
        public int? CustomerId { get; set; }
        public int? SupplierId { get; set; }
        public string Miscellaneous { get; set; }
        public bool IsActive { get; set; }
        public int AccountType { get; set; }
        public int AccountLedgerId { get; set; }
        public string IpAddress { get; set; }
        public int IsDeleted { get; set; }

        [Write(false)]
        public decimal LedgerBalance { get; set; }        

        [Write(false)]
        public string PayeeTo { get; set; } 

        [Write(false)]
        public double SubTotal { get; set; } 
        [Write(false)]
        public string BranchName { get; set; }

        [Write(false)]
        public List<Select2OptionModel> BranchList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> SupplierList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> AccountLedgerList { get; set; }

        [Write(false)]
        public List<AccountVoucherDetails> AccountVoucherDetails { get; set; }
    }
}
