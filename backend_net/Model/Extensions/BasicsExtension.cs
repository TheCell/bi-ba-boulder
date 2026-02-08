using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model.Basics;

namespace Thecell.Bibaboulder.Model.Extensions;

public static class BasicsExtension
{
    //public static AuditableEntity FillAuditFieldsForTesting(this AuditableEntity auditableEntity)
    //{
    //    auditableEntity.CreatedDate = DateTime.UtcNow;
    //    auditableEntity.CreatedUserId = Guid.NewGuid();
    //    return auditableEntity;
    //}

    public static async Task<T> ThrowIfNullAsync<T>(this Task<T?> entityTask, Guid? id = null) where T : EntityAuditFields
    {
        var entity = await entityTask;

        if (entity is null)
        {
            NotFoundException.ThrowIfNull(entity, typeof(T).Name, id);
        }

        return entity;
    }
}
