using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Products
{
    [Table("AttributeValues")]
    public class AttributeValue : IBaseEntity
    {
        [Key]
        public int Id { get; set; }
        public int AttributeId { get; set; }
        public string Value { get; set; }
        public string ColorCode { get; set; }
        [Write(false)]
        public string AttributeName { get; set; }

        [Write(false)]
        public EntityState EntityState { get; set; }
    }
}