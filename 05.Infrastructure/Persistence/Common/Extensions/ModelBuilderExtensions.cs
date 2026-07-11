using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Smlp.Domain.Interfaces;

namespace Smlp.Infrastructure.Persistence.Common.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyConfigurationsFromNameSpace(this ModelBuilder modelBuilder, string configurationNamespace)
    {
        var applyGenericMethods = typeof(ModelBuilder).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        var applyGenericApplyConfigurationMethods = applyGenericMethods
            .Where(m => m.IsGenericMethod && m.Name.Equals("ApplyConfiguration", StringComparison.OrdinalIgnoreCase));
        var applyGenericMethod = applyGenericApplyConfigurationMethods
            .Where(m => m.GetParameters().FirstOrDefault()?.ParameterType.Name == "IEntityTypeConfiguration`1")
            .FirstOrDefault();

        var applicableTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(c => c.IsClass && !c.IsAbstract && !c.ContainsGenericParameters && c.Namespace == configurationNamespace);

        foreach (var applicableType in applicableTypes)
        {
            foreach (var interfaceType in applicableType.GetInterfaces())
            {
                if (interfaceType.IsConstructedGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                {
                    var applyConcreteMethod = applyGenericMethod?.MakeGenericMethod(interfaceType.GenericTypeArguments[0]);
                    var instance = Activator.CreateInstance(applicableType);
                    if (instance is not null)
                        applyConcreteMethod?.Invoke(modelBuilder, new object[] { instance });
                    break;
                }
            }
        }
    }

    public static void SetSoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IDeletable).IsAssignableFrom(entityType.ClrType)) continue;

            // EF Core only allows a query filter to be declared on the root entity
            // type of a mapping hierarchy; derived types inherit it automatically.
            // Skipping non-root types here also guards against accidental shared
            // hierarchies (e.g. two unrelated entities that happen to share a
            // common abstract base class) throwing at model-build time.
            if (entityType.BaseType is not null) continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(IDeletable.IsDeleted));
            var falseConstant = Expression.Constant(false);
            var filterExpression = Expression.Equal(property, falseConstant);
            var lambda = Expression.Lambda(filterExpression, parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
