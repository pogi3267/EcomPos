using System;
namespace ApplicationCore.Entities.ApplicationUser
{
    public class BaseModel
    {
        public BaseModel()
        {
            CreatedOn = DateTime.UtcNow;
        }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
