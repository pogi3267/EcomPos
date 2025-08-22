using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Inventory
{
    [Table("Bank")]
    public class Bank:BaseEntity
    {
        [Key]
        public int BankId { get; set; }
        public string BankName { get; set; }
        public string BankAddress { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNo { get; set; }
    }
}