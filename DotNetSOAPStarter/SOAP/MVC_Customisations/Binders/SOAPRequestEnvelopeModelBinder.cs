using DotNetSOAPStarter.SOAP.Attributes;
using DotNetSOAPStarter.SOAP.Model;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DotNetSOAPStarter.SOAP.MVC_Customisations.Binders
{
    // This allows a controller action to receive a SOAPRequestEnvelope rather than a 
    // SOAP version specific SOAP?_?RequestEnvelope
    public class SOAPRequestEnvelopeModelBinder : IModelBinder
    {
        protected Dictionary<Type, (ModelMetadata, IModelBinder)> _binders;
        public SOAPRequestEnvelopeModelBinder(Dictionary<Type, (ModelMetadata, IModelBinder)> binders)
        {
            _binders = binders;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            SOAPControllerAttribute? soapAttribute = bindingContext.ActionContext.ActionDescriptor.EndpointMetadata.OfType<SOAPControllerAttribute>().FirstOrDefault();
            if (soapAttribute is null)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }
            Type requestModelType = soapAttribute.SOAPVersion == SOAPVersion.v1_1 ? typeof(SOAP1_1RequestEnvelope) : typeof(SOAP1_2RequestEnvelope);
            (ModelMetadata metaData, IModelBinder binder) modelClasses = _binders[requestModelType];
            ModelMetadata requestModelMetaData = modelClasses.metaData;
            IModelBinder requestModelBinder = modelClasses.binder;

            var requestBindingContext = DefaultModelBindingContext.CreateBindingContext(
                bindingContext.ActionContext,
                bindingContext.ValueProvider,
                requestModelMetaData,
                bindingInfo: null,
                bindingContext.ModelName);
            await requestModelBinder.BindModelAsync(requestBindingContext);
            
            bindingContext.Result = requestBindingContext.Result;
            if (requestBindingContext.Result.IsModelSet && requestBindingContext.Result.Model is not null)
            {
                // Setting the ValidationState ensures properties on derived types are correct 
                bindingContext.ValidationState[requestBindingContext.Result.Model] = new ValidationStateEntry
                {
                    Metadata = requestModelMetaData,
                };
            }
        }
    }

    class SOAPRequestEnvelopeModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType != typeof(SOAPRequestEnvelope))
                return null;
            var subclasses = new[] { typeof(SOAP1_1RequestEnvelope), typeof(SOAP1_2RequestEnvelope) };
            var binders = new Dictionary<Type, (ModelMetadata, IModelBinder)>();
            foreach (var type in subclasses)
            {
                var modelMetadata = context.MetadataProvider.GetMetadataForType(type);
                binders[type] = (modelMetadata,
                context.CreateBinder(modelMetadata, new BindingInfo() { BindingSource = BindingSource.Body, EmptyBodyBehavior = EmptyBodyBehavior.Allow }));
            }
            return new SOAPRequestEnvelopeModelBinder(binders);
        }
    }
}
