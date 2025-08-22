using System;
using System.Collections.Generic;

namespace ApplicationCore.DTOs
{
    public class InvoiceOrderDTO
    {
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public int PickupPointId { get; set; }
        public string ReceiverName { get; set; }
        public string FullAddress { get; set; }
        public string Phone { get; set; }
        public string AddressType { get; set; }
        public string PaymentType { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentDetails { get; set; }
        public string CustomerName { get; set; }
        public string CustomerUserName { get; set; }
        public string PhoneNumber { get; set; }
        public List<InvoiceOrderDetail> OrderDetails { get; set; }
        public GeneralSettings CompanyInfo { get; set; }
    }
    public class InvoiceOrderDetail
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingType { get; set; }
    }
    public class GeneralSettings
    {
        public string SystemName { get; set; }
        public string SystemLogoWhite { get; set; }
        public string PhoneNumber { get; set; }
        public string TelephonNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }

}


