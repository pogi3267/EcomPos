using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Identity;
using System;

namespace ApplicationCore.Entities.ApplicationUser
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Discriminator { get; set; }

        #region Additional
        [Write(false)]
        public string Token { get; set; }
        [Write(false)]
        public string Role { get; set; }
        [Write(false)]
        public string Avatar { get; set; }
        [Write(false)]
        public string AvatarOriginal { get; set; }
        [Write(false)]
        public string Address { get; set; }
        [Write(false)]
        public string Country { get; set; }
        [Write(false)]
        public string State { get; set; }
        [Write(false)]
        public string City { get; set; }
        [Write(false)]
        public string PostalCode { get; set; }
        [Write(false)]
        public string ReferredBy { get; set; }
        [Write(false)]
        public string ProviderId { get; set; }

        //public decimal Balance { get; set; }
        [Write(false)]
        public Int16? Banned { get; set; }
        [Write(false)]
        public string ReferralCode { get; set; }
        [Write(false)]
        public int? CustomerPackageId { get; set; }
        [Write(false)]
        public string RemainingUploads { get; set; }
        [Write(false)]
        public DateTime? CreatedDate { get; set; }
        [Write(false)]
        public DateTime? UpdatedDate { get; set; }

        #endregion Additional
    }
}