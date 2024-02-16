using DotNetSOAPStarter.SOAP.Controllers;
using DotNetSOAPStarter.SOAP.Model;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DotNetSOAPStarter.SOAP.Filters
{
    public class PayloadRequiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!typeof(SOAPControllerBase).IsAssignableFrom(context.Controller.GetType()))
            {
                throw new Exception("PayloadRequiredAttribute can only be used on classes deriving from SOAPControllerBase");
            }

            SOAPControllerBase controller = (SOAPControllerBase)context.Controller;

            var parametersMetaData = context.ActionDescriptor.Parameters
                .Where(x =>
                typeof(SOAPRequestEnvelope).IsAssignableTo(x.ParameterType) ||
                typeof(SOAP1_1RequestEnvelope).IsAssignableTo(x.ParameterType) || 
                typeof(SOAP1_2RequestEnvelope).IsAssignableTo(x.ParameterType));
            
            if (parametersMetaData.Count() < 1)
            {
                if (context.HttpContext.Request.ContentLength == 0)
                {
                    context.Result = controller.SOAPPayloadMissing();
                }

                return;
            }

            if (parametersMetaData.Count() > 1)
            {
                throw new Exception($"PayloadRequiredAttribute cannot determine the target model type. Too many parameters of type {nameof(SOAPRequestEnvelope)}, {nameof(SOAP1_1RequestEnvelope)} or {nameof(SOAP1_2RequestEnvelope)}");
            }

            var parameterMetaData = parametersMetaData.First();
            
            if (parameterMetaData is not null)
            {
                var payload = context.ActionArguments
                    .Where(x => x.Value is SOAPRequestEnvelope)
                    .Select(v => (SOAPRequestEnvelope?)(v.Value))
                    .FirstOrDefault();

                if (payload is null || payload.Body is null)
                {
                    context.Result = controller.SOAPPayloadMissing();
                }
            }
        }
    }
}
