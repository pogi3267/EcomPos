using System;

namespace ApplicationCore.DTOs
{
    public class RepDateWiseCollectionDTO
    {
       
        public string Invoice { get; set; }
        public DateTime CollectionDate { get; set; }
        public string CustomerName { get; set; }
        public string Type { get; set; }
        public int BankName { get; set; }
        public string CheckNumber { get; set; }
        public string ColRemark { get; set; }
        public decimal Amount { get; set; }
    }
    public class RepInvoiceCollectionDTO
    {

        public string InvoiceNoCollection { get; set; }
        public DateTime CollectionDate { get; set; }
        public int CustomerId { get; set; }
        public string Cltype { get; set; }
        public string Collectiontype { get; set; }
        public string EntryBy { get; set; }
        public string BankName { get; set; }
        public string ChequeNo { get; set; }
        public DateTime ChequeDate { get; set; }
        public string ColRemark { get; set; }
        public string BankaccountNo { get; set; }
        public string GrohonKorece { get; set; }
        public string OrganizationName { get; set; }
        public string Address { get; set; }
        public decimal CollectionAmount { get; set; }
    }
}

