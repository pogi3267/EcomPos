using ApplicationCore.Enums;

namespace ApplicationCore.Interfaces
{
    public interface IBaseEntity
    {
        EntityState EntityState { get; set; }
    }
}
