using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Psmp.WebApi.Common.ModelBindings;

public class JsonModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext is null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var modelBindingKey = bindingContext.IsTopLevelObject ? bindingContext.BinderModelName ?? string.Empty : bindingContext.ModelName;

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult != ValueProviderResult.None)
        {
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            var valueAsString = valueProviderResult.FirstValue;

            if (valueAsString is null)
            {
                return Task.CompletedTask;
            }

            object? result;

            try
            {
                result = JsonConvert.DeserializeObject(valueAsString, bindingContext.ModelType);
            }
            catch (JsonReaderException exception)
            {
                bindingContext.ModelState.TryAddModelException(modelBindingKey, exception);

                return Task.CompletedTask;
            }

            if (result is not null)
            {
                bindingContext.Result = ModelBindingResult.Success(result);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}
