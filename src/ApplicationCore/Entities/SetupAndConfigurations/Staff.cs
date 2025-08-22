using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.SetupAndConfigurations
{
    [Table("Staffs")]
    public class Staff : BaseEntity
    {
        [Key]
        public int StaffId { get; set; }
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public DateTime? Deleted_At { get; set; }
        [Write(false)]
        public string Name { get; set; }
        [Write(false)]
        public string Email { get; set; }
        [Write(false)]
        public string Phone { get; set; }
        [Write(false)]
        public string Password { get; set; }
        [Write(false)]
        public string Role { get; set; }
        [Write(false)]
        public List<Select2OptionModel> RoleList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> UserList { get; set; }
    }
}