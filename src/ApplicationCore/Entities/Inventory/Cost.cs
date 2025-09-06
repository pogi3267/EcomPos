using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Inventory
{
    [Table("Cost")]
    public class Cost : BaseEntity
    {
        [Key]
        public int CostId { get; set; }
        public string Description { get; set; }
    }

}





