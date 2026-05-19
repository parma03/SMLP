using System.Reflection;
using Microsoft.OpenApi.Models;
using Smlp.Shared.Common.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Smlp.WebApi.Services.Documentation.Swagger.Filters;

public class OpenApiContentTypeOperationFilter : IOperationFilter
{
    private IDictionary<string, string> ParameterNames { get; set; } = new Dictionary<string, string>();

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ParameterNames = new Dictionary<string, string>();

        var parameters = context.MethodInfo.GetParameters();

        foreach (var parameter in parameters)
        {
            if (parameter.ParameterType.IsClass)
            {
                var typeInfo = parameter.ParameterType.GetTypeInfo();

                DeclaredPropertyByParameterName(typeInfo, operation);
            }
        }

        var contentTypeByParameterName = parameters
            .Where(p => p.IsDefined(typeof(OpenApiContentTypeAttribute), true))
            .ToDictionary(p => p.Name!, s => s.GetCustomAttribute<OpenApiContentTypeAttribute>()!.ContentType);

        if (contentTypeByParameterName.Any())
        {
            foreach (var requestContent in operation.RequestBody.Content)
            {
                var encodings = requestContent.Value.Encoding;

                foreach (var encoding in encodings)
                {
                    if (contentTypeByParameterName.TryGetValue(encoding.Key, out var value))
                    {
                        encoding.Value.ContentType = value;
                    }
                }
            }
        }
    }

    private void DeclaredPropertyByParameterName(TypeInfo typeInfo, OpenApiOperation operation, string propertyName = null!)
    {
        foreach (var declaredProperty in typeInfo.DeclaredProperties)
        {
            var propertyType = declaredProperty.PropertyType;

            if (propertyType.IsClass && !propertyType.FullName!.StartsWith("System."))
            {
                var propertyTypeInfo = propertyType.GetTypeInfo();

                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    DeclaredPropertyByParameterName(propertyTypeInfo, operation, propertyType.Name);
                }
                else
                {
                    DeclaredPropertyByParameterName(propertyTypeInfo, operation, $"{propertyName}.{propertyType.Name}");
                }
            }
            else
            {
                if (declaredProperty.IsDefined(typeof(OpenApiContentTypeAttribute)))
                {
                    ParameterNames.Add(
                        string.IsNullOrWhiteSpace(propertyName)
                        ? declaredProperty.Name
                        : $"{propertyName}.{declaredProperty.Name}",
                        declaredProperty.GetCustomAttribute<OpenApiContentTypeAttribute>()!.ContentType);
                }
            }
        }

        if (ParameterNames.Any() && operation.RequestBody is not null)
        {
            foreach (var requestContent in operation.RequestBody.Content)
            {
                var encodings = requestContent.Value.Encoding;

                foreach (var encoding in encodings)
                {
                    if (ParameterNames.TryGetValue(encoding.Key, out var value))
                    {
                        encoding.Value.ContentType = value;
                    }
                }
            }
        }
    }
}
