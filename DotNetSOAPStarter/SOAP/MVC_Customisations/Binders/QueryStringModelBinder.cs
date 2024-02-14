using DotNetSOAPStarter.SOAP.MVC_Customisations.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DotNetSOAPStarter.SOAP.MVC_Customisations.Binders
{
    // This binder allows us to determine if a query string parameter is present on the url and whether it has a value or not
    // e.g. 
    // {url}?test - will bind to parameter string? test with a value of empty string
    // {url}?test=hello - will bind to parameter string? test with a value of 'hello'
    // {url}? - will bind to parameter string? test with a value of null
    // So 
    // Null means the parameter was not in the query string
    // Empty string means the parameter was in the query string without any value
    // A string value means the parameter was in the query string and had a value
    class QueryStringNullOrEmptyModelBinder : IModelBinder
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
                    // Value is something else, fail
                    bindingContext.ModelState.TryAddModelError(
                        bindingContext.ModelName,
                        "Value must be a string or null");
                }
            }

            return Task.CompletedTask;
        }
    }
}

class QueryStringNullOrEmptyModelBinderProvider : IModelBinderProvider
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
