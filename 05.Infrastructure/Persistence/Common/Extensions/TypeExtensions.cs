using System.Reflection;
using Smlp.Domain.Abstracts;

namespace Smlp.Infrastructure.Persistence.Common.Extensions;

public static class TypeExtensions
{
    public static bool IsAuditableEntity(this Type entity)
    {
        var generic = typeof(AuditableEntity);

        while (entity is not null && entity != typeof(object))
        {
            var currentType = entity.GetTypeInfo().IsGenericType ? entity.GetGenericTypeDefinition() : entity;

            if (generic == currentType)
            {
                return true;
            }

            var baseType = entity.GetTypeInfo().BaseType;

            if (baseType is null)
            {
                break;
            }

            entity = baseType;
        }

        return false;
    }
}
