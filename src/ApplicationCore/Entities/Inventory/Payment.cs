using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.Inventory
{
    [Table("Payment")]
    public class Payment : BaseEntity
    {
        [Key]
        public long PaymentId { get; set; }
        public string PayInvoiceNo { get; set; }
        public DateTime? PayDate { get; set; }
        public int SupplierId { get; set; }
        public string Cltype { get; set; }
        public string Remarks { get; set; }
        public string PayType { get; set; }
        public int BankId { get; set; }
        public string AccountNo { get; set; }
        public string ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public double? PayAmount { get; set; }
        public DateTime? NextPayDate { get; set; }
        public string EntryIp { get; set; }
        public string EntryBy { get; set; }
        public string ApproveState { get; set; }
        public int Approve { get; set; }
        public string ApproveBy { get; set; }
        public DateTime? ApproveDate { get; set; }
        public string Department { get; set; }
        public string CompanyName { get; set; }
        public string MessageStatus { get; set; }
        public string ApproveRemarks { get; set; }
        public int BranchId { get; set; }

        [Write(false)]
        public int BankAccountId { get; set; }
       
        [Write(false)]
        public string SupplierName { get; set; } 
        [Write(false)]
        public string BankName { get; set; }
        [Write(false)]
        public string SupplierCode { get; set; }

        [Write(false)]
        public List<Select2OptionModel> SupplierList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> BranchList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> BankList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> BankAccountList { get; set; }
    
    }
}
