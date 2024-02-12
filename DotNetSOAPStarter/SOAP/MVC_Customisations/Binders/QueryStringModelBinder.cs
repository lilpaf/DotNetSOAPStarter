using DotNetSOAPStarter.SOAP.MVC_Customisations.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DotNetSOAPStarter.SOAP.MVC_Customisations.Binders
{
    public class QueryStringNullOrEmptyModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (result == ValueProviderResult.None)
            {
                // Parameter is missing, interpret as null
                bindingContext.Result = ModelBindingResult.Success(null);
            }
            else
            {
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, result);
                var rawValue = result.FirstValue;
                if (string.IsNullOrEmpty(rawValue)) 
                {
                    // Value is empty, interpret as empty string
                    bindingContext.Result = ModelBindingResult.Success(string.Empty);
                }
                else if (rawValue is string) 
                {
                    // Value is a valid string, use that value
                    bindingContext.Result = ModelBindingResult.Success(rawValue);
                }
                else
                {
                    bindingContext.ModelState.TryAddModelError(
                        bindingContext.ModelName,
                        "Value must be a string or null");
                }
            }

            return Task.CompletedTask;
        }
    }
}

public class QueryStringNullOrEmptyModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(string) &&
            context.BindingInfo.BindingSource != null &&
            context.BindingInfo.BindingSource.CanAcceptDataFrom(BindingSource.Query))
        {
            return new QueryStringNullOrEmptyModelBinder();
        }

        return null;
    }

    
}
