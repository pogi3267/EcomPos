using System;

namespace ApplicationCore.Entities.Orders
{
    public class OrderNotification
    {
        public int NotificationId { get; set; }
        public int OrdersId { get; set; }
        public string NotificationMessage { get; set; }
        public string TimeSinceNotification { get; set; }
        public DateTime NotificationDate { get; set; }
        public bool IsView { get; set; }

    }
}








