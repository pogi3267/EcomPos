using ApplicationCore.Entities;

namespace ApplicationCore.DTOs
{
    public class HomeBusinessAnalyticsDTO : BaseEntity
    {

        public int TotalOrders { get; set; }
        public int TotalStores { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }
        public int Pending { get; set; }
        public int Packaging { get; set; }
        public int Delivered { get; set; }
        public int Returned { get; set; }
        public int Confirmed { get; set; }
        public int OutForDelivery { get; set; }
        public int Canceled { get; set; }
        public int FailedToDeliver { get; set; }

    }
    public class SaleForGraph
    {
        public int OrderDate { get; set; }
        public int QtyOfOrders { get; set; }
    }
    public class SalesDataDTO
    {
        public int OrderMonth { get; set; }
        public int OrderYear { get; set; }
        public decimal TotalAmounts { get; set; }
        public int YearIndicator { get; set; }
    }
    public class PaymentDataDTO
    {

        public string PaymentType { get; set; }
        public decimal TotalAmount { get; set; }
    }
}

