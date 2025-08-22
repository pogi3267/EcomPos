using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Inventory
{
    [Table("Branch")]
    public class Branch: BaseEntity
    {
        [Key]
        public int BranchId {get; set; }
        public string BranchName {get; set; }
        public string Address {get; set; }
        public string ManagerName {get; set; }
        public string VatRegistrationNumber {get; set; }
        public string Phone {get; set; }
        public string BranchMarking {get; set; }
        public string WebAddress {get; set; }
        public decimal OpeningBalance {get; set; }
        public decimal BranchInitial {get; set; }
    }
}