using Dapper.Contrib.Extensions;
using System;

namespace ApplicationCore.Entities.Products
{
    [Table("Units")]
    public class Unit : BaseEntity
    {
        [Key]
        public int UnitId { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public DateTime? Deleted_At { get; set; }

    }
}
