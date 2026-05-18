using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Psmp.Shared.Common.Attributes;
using Psmp.Shared.Common.Constants;

namespace Psmp.WebApi.Common.ModelBindings;

public class JsonModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var propertyName = context.Metadata.PropertyName;

        if (propertyName is null)
        {
            return null;
        }

        var propertyInfo = context.Metadata.ContainerType?.GetProperty(propertyName);

        if (propertyInfo is null)
        {
            return null;
        }

        var attribute = propertyInfo.GetCustomAttribute<OpenApiContentTypeAttribute>();

        if (attribute is not null && attribute.ContentType == ContentTypes.ApplicationJson)
        {
            return new JsonModelBinder();
        }
        else
        {
            return null;
        }
    }
}
