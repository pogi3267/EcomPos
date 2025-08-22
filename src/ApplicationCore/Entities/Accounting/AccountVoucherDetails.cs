using ApplicationCore.Enums;
using Dapper.Contrib.Extensions;
using System;

namespace ApplicationCore.Entities.Accounting
{
    [Table("AccountVoucherDetails")]
    public class AccountVoucherDetails : BaseEntity
    {
        public AccountVoucherDetails()
        {
            IsActive = true;
        }
        [Key]
        public int AccountVoucherDetailId { get; set; }
        public int AccountVoucherId { get; set; }
        public AmountType TypeId { get; set; }
        public int? ChildId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public DateTime VoucherDate { get; set; }
        public string Reference { get; set; }
        public bool IsActive { get; set; }
        public int? BranchId { get; set; }
        public int IsDeleted { get; set; }

        [Write(false)]
        public int VoucherDetailsId { get; set; }
    }
}
