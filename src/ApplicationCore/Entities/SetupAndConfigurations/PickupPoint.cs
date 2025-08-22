using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.SetupAndConfigurations
{
    [Table("PickupPoints")]
    public class PickupPoint : BaseEntity
    {
        [Key]
        public int PickupPointId { get; set; }
        public int StaffId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public bool PickUpStatus { get; set; }
        public int? CashOnPickupStatus { get; set; }
        public DateTime? Deleted_At { get; set; }
        [Write(false)]
        public List<Select2OptionModel> StaffList { get; set; }
        [Write(false)]
        public string StaffName { get; set; }

    }
}
