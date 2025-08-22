using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.ApplicationUser
{
    public class User
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
        public bool Remember { get; set; }
        public string ConfirmPassword { get; set; }
        public string NewPassword { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public IFormFile AvatarImage { get; set; }
        public string AvatarOriginal { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string ReferredBy { get; set; }
        public string ProviderId { get; set; }
        public Int16? Banned { get; set; }
        public string ReferralCode { get; set; }
        public int? CustomerPackageId { get; set; }
        public string RemainingUploads { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int TotalRows { get; set; }
        public int TotalOrder { get; set; }
        public int OngoingOrder { get; set; }
        public int CompleteOrder { get; set; }
        public int ReturnedOrder { get; set; }
        public int FailedToDeliverOrder { get; set; }
        public int CanceledOrder { get; set; }
        public bool IsDecrypt { get; set; }
        public bool IsClaimNeeded { get; set; }
        public List<Orders.Orders> Orders { get; set; }
    }
}