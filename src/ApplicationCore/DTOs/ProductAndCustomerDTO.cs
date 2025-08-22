using System;
using System.Collections.Generic;

namespace ApplicationCore.DTOs
{
    public class ProductAndCustomerDTO
    {
        public List<ProductDTO> RecentlyCreatedProducts { get; set; }
        public List<ProductDTO> TopSellingItems { get; set; }
        public List<CustomerDTO> TopCustomer { get; set; }
    }

    public class ProductDTO
    {
        public int ProductID { get; set; }
        public decimal UnitPrice { get; set; }
        public string Name { get; set; }
        public string ThumbnailFileName { get; set; }
        public string Description { get; set; }
        public decimal TotalQuantitySold { get; set; }
        public DateTime Created_At { get; set; }
    }

    public class CustomerDTO
    {
        public string Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerImg { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public decimal TotalSpent { get; set; }
    }
}


