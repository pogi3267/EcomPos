using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ApplicationCore.Entities.Inventory
{
    [Table("OperationalUser")]
    public class OperationalUser : BaseEntity
    {
        public OperationalUser()
        {
           
        }
        [Key]
        public int OperationalUserId { get; set; } 
        public string Code { get; set; }
        public string OrganizationName { get; set; }
        public string Department { get; set; }
        public string ImageUrl { get; set; }
        public string ContactPerson { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public decimal OpeningBalance { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }

        [Write(false)]
        public IFormFile Photo { get; set; }

        [Write(false)]
        public string ListName { get; set; }
        [Write(false)]
        public string CreateNew { get; set; }
    }
}

