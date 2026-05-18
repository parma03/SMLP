using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Psmp.Infrastructure.Persistence.Common.Extensions;

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
                // Check if the type implements interface IEntityTypeConfiguration<SomeEntity>.
                if (interfaceType.IsConstructedGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                {
                    // Make concrete ApplyConfiguration<SomeEntity> method.
                    var applyConcreteMethod = applyGenericMethod?.MakeGenericMethod(interfaceType.GenericTypeArguments[0]);

                    // And invoke that with fresh instance of your configuration type.

                    var instance = Activator.CreateInstance(applicableType);

                    if (instance is not null)
                    {
                        applyConcreteMethod?.Invoke(modelBuilder, new object[] { instance });
                    }

                    break;
                }
            }
        }
    }
}
