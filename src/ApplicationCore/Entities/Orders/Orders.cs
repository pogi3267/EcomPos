using ApplicationCore.Entities.ApplicationUser;
using ApplicationCore.Entities.Products;
using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.Orders
{
    [Table("Orders")]
    public class Orders : BaseEntity
    {
        public Orders()
        {
            PaymentStatus = "";
            OrderDetailsList = new List<OrderDetail>();
            Address = new Address();
        }

        [Key]
        public int OrdersId { get; set; }
        public int? CombinedOrdersId { get; set; }
        public string UserId { get; set; }
        public string GuestId { get; set; }
        public string SellerId { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingType { get; set; }
        public int PickupPointId { get; set; }
        public string DeliveryStatus { get; set; }
        public string PaymentType { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentDetails { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal TotalAdminDiscount { get; set; }
        public decimal CouponDiscount { get; set; }
        public string Code { get; set; }
        public string TrackingCode { get; set; }
        public string CourierName { get; set; }
        public string CourierTrackingNo { get; set; }
        public DateTime Date { get; set; }
        public bool Viewed { get; set; }
        public bool DeliveryViewed { get; set; }
        public bool PaymentStatusViewed { get; set; }
        public bool CommissionCalculated { get; set; }
        [Write(false)]
        public string CustomerName { get; set; }
        [Write(false)]
        public string CustomerUserName { get; set; }
        [Write(false)]
        public string Email { get; set; }
        [Write(false)]
        public string PhoneNumber { get; set; }
        [Write(false)]
        public string DateToString
        {
            get => this.Date.ToString("dd MMM yyyy");
        }

        [Write(false)]
        public bool IsPaymentStatusPaid
        {
            get => this.PaymentStatus.ToLower() == "paid" ? true : false;
            set => this.IsPaymentStatusPaid = value;
        }

        [Write(false)]
        public List<OrderDetail> OrderDetailsList { get; set; }

        [Write(false)]
        public Address Address { get; set; }

        [Write(false)]
        public string PickupPointName { get; set; }

        [Write(false)]
        public string PickupPointPhone { get; set; }

        [Write(false)]
        public string PickupPointAddress { get; set; }
        [Write(false)]
        public List<Product> ProductList { get; set; }

    }

    public static class OrderStatus
    {
        public const string Pending = "Pending";
        public const string Confirmed = "Confirmed";
        public const string Packaging = "Packaging";
        public const string OutForDelivery = "OutForDelivery";
        public const string Delivered = "Delivered";
        public const string Returned = "Returned";
        public const string FailedToDeliver = "FailedToDeliver";
        public const string Canceled = "Canceled";
    }

    public static class ShippingType
    {
        public const string BySelfDeliveryMan = "BySelfDeliveryMan";
        public const string ByThirdPartyDeliveryService = "ByThirdPartyDeliveryService";
    }

    public static class OrderHelper
    {
        public static string GetOrderStatus(string status)
        {
            if (status == OrderStatus.Pending)
            {
                return "Pending";
            }
            else if (status == OrderStatus.Confirmed)
            {
                return "Confirmed";
            }
            else if (status == OrderStatus.Packaging)
            {
                return "Packaging";
            }
            else if (status == OrderStatus.OutForDelivery)
            {
                return "Out For Delivery";
            }
            else if (status == OrderStatus.Delivered)
            {
                return "Delivered";
            }
            else if (status == OrderStatus.Returned)
            {
                return "Returned";
            }
            else if (status == OrderStatus.FailedToDeliver)
            {
                return "Failed To Deliver";
            }
            else if (status == OrderStatus.Canceled)
            {
                return "Canceled";
            }
            return "";
        }

        public static string GetShippingType(string type)
        {
            if (type == ShippingType.BySelfDeliveryMan)
            {
                return "By Self Delivery Man";
            }
            else if (type == ShippingType.ByThirdPartyDeliveryService)
            {
                return "By Third Party Delivery Service";
            }
            return "";
        }
    }
}


