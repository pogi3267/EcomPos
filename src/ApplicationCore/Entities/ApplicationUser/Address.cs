using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Dapper.Contrib.Extensions;
using System;

namespace ApplicationCore.Entities.ApplicationUser
{
    [Table("Addresses")]
    public class Address : IBaseEntity
    {
        [Key]
        public int AddressId { get; set; }

        public string UserId { get; set; }

        public string Address1 { get; set; }

        public string Country { get; set; }

        public string State { get; set; }

        public string City { get; set; }

        public float? Longitude { get; set; }

        public float? Latitude { get; set; }

        public string PostalCode { get; set; }

        public string AddressType { get; set; }

        public string ReceiverName { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public bool SetDefault { get; set; }

        public DateTime Created_At { get; set; }

        public DateTime Updated_At { get; set; }

        [Write(false)]
        public EntityState EntityState { get; set; }

    }
}