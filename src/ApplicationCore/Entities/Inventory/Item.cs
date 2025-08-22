using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Inventory
{
    [Table("Item")]
    public class Item : BaseEntity
    {
        [Key]
        public int ItemId { get; set; }
        public string ItemType { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public string CompanyName { get; set; }
    }
}