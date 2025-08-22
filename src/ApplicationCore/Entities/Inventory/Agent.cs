using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Inventory
{
    [Table("Agent")]
    public class Agent: BaseEntity
    {
        [Key]
        public int AgentId { get; set; }
        public string AgentName { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Remarks { get; set; }
        public decimal Amount { get; set; }
    }
}