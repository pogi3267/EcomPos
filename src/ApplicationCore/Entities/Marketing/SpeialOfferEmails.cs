using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Marketing
{
    [Table("SpecialOfferEmails")]
    public class SpecialOfferEmails : IBaseEntity
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        [Write(false)]
        public EntityState EntityState { get; set; }
        [Write(false)]
        public int TotalRows { get; set; }
    }
}