using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.Inventory
{
    [Table("Collection")]
    public class Collection : BaseEntity
    {
        public Collection()
        {
            DepositDate=DateTime.Now;

        }

        [Key]
        public long CollectionId { get; set; }
        public string InvoiceNoCollection { get; set; }
        public DateTime? CollectionDate { get; set; }
        public int CustomerId { get; set; }
        public string ClType { get; set; }
        public string Collectiontype { get; set; }
        public int BankId { get; set; }
        public string ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public double? CollectionAmount { get; set; }
        public string ColRemark { get; set; }
        public string MessageStatus { get; set; }
        public string FinalizedStatus { get; set; }
        public string FinalizedRemark { get; set; }
        public string EntryBy { get; set; }
        public string FinalizedBy { get; set; }
        public DateTime? FinalizedDate { get; set; }
        public bool Approved { get; set; }
        public string Department { get; set; }
        public string CompanyName { get; set; }
        public string SalesNo { get; set; }
        public int BranchId { get; set; }

        #region Deposit 
        public int? DepositBankId { get; set; }
        public int? DepositAccountNumberId { get; set; }
        public DateTime? DepositDate { get; set; }
        public string DepositRemark { get; set; }
        public string DepositStatus { get; set; }
        public string DepositEntryBy { get; set; }
        #endregion

        [Write(false)]
        public string CustomerName { get; set; }
        [Write(false)]
        public string CustomerCode { get; set; }
        [Write(false)]
        public string BankName { get; set; }
        [Write(false)]
        public string DepositBankName { get; set; }
        [Write(false)]
        public string DepositAccountNumber { get; set; }

        [Write(false)]
        public List<Select2OptionModel> CustomerList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> BankList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> DepositBankList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> DepositAccountNumberList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> BranchList { get; set; }
    }
}