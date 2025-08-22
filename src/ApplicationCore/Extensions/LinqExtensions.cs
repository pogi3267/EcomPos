using ApplicationCore.Entities;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using System.Collections.Generic;

namespace ApplicationCore.Extensions
{
    public static class LinqExtensions
    {
        public static void SetUnchanged(this IEnumerable<BaseEntity> list)
        {
            foreach (var item in list)
            {
                item.EntityState = EntityState.Unchanged;
            }
        }

        public static void SetAdded(this IEnumerable<BaseEntity> list)
        {
            foreach (var item in list)
            {
                item.EntityState = EntityState.Added;
            }
        }

        public static void SetModified(this IEnumerable<BaseEntity> list)
        {
            foreach (var item in list)
            {
                item.EntityState = EntityState.Modified;
            }
        }

        public static void SetDeleted(this IEnumerable<BaseEntity> list)
        {
            foreach (var item in list)
            {
                item.EntityState = EntityState.Deleted;
            }
        }

        public static void SetUnchanged(this IEnumerable<IBaseEntity> list)
        {
            foreach (var item in list)
            {
                item.EntityState = EntityState.Unchanged;
            }
        }

        public static void SetAdded(this IEnumerable<IBaseEntity> list)
        {
            foreach (var item in list)
            {
                item.EntityState = EntityState.Added;
            }
        }

        public static void SetModified(this IEnumerable<IBaseEntity> list)
        {
            foreach (var item in list)
            {
                item.EntityState = EntityState.Modified;
            }
        }

        public static void SetDeleted(this IEnumerable<IBaseEntity> list)
        {
            foreach (var item in list)
            {
                item.EntityState = EntityState.Deleted;
            }
        }

    }
}
