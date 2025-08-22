using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Dapper.Contrib.Extensions;
using System;

namespace ApplicationCore.Entities
{
    public abstract class BaseEntity : IBaseEntity
    {
        [Write(false)]
        public EntityState EntityState { get; set; }
        [Write(false)]
        public int TotalRows { get; set; }
        public BaseEntity()
        {
            EntityState = EntityState.Added;
            Created_At = DateTime.UtcNow;
        }
        public DateTime Created_At { get; set; }
        public DateTime? Updated_At { get; set; }
        public string Created_By { get; set; }
        public string Updated_By { get; set; }
    }
    public class Select2OptionModel
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
    }
    public class CategorySelect2Option : Select2OptionModel
    {
        public int ParentId { get; set; }
    }
    public class ColorSelect2Option : Select2OptionModel
    {
        public string Code { get; set; }
    }
}
