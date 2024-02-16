using DotNetSOAPStarter.SOAP.Attributes;
using DotNetSOAPStarter.SOAP.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SOAP;

namespace DotNetSOAPStarter.SOAP.MVC_Customisations.DependencyInjection
{
    public class ApiBehaviorOptionsExtensions
    {
        public static IActionResult InvalidModelStateResponseFactory(ActionContext actionContext)
        {
            /*
             * Here we can customize the default error response e.g. SOAP Version by Controller, along with SOAP:Fault detail element
            */
            string? faultString = actionContext.ModelState.First(e => e.Value?.Errors.Count > 0).Value?.Errors.First().ErrorMessage;

            if (faultString is null)
            {
                faultString = "An unexpected error occurred.";
            }

            SOAPControllerAttribute? soapAttribute = actionContext.ActionDescriptor.EndpointMetadata.OfType<SOAPControllerAttribute>().FirstOrDefault();

            // Must return a SOAP Fault
            if (soapAttribute is not null)
            {
                return soapAttribute.SOAPVersion == SOAPVersion.v1_1 ?
                    SOAPEnvelopeResponses.SOAPFault(new SOAP1_1ResponseEnvelope(), faultString) :
                    SOAPEnvelopeResponses.SOAPFault(new SOAP1_2ResponseEnvelope(), new Reason(faultString));
            }

            // Must be REST so return the default WebAPI ProblemDetails object response
            var handler = actionContext.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

            if (actionContext.ModelState.ValidationState == ModelValidationState.Invalid)
            {
                var validationProblem = handler.CreateValidationProblemDetails(actionContext.HttpContext, actionContext.ModelState);
                return new BadRequestObjectResult(validationProblem);
            }

            var problem = handler.CreateProblemDetails(actionContext.HttpContext, 500, faultString, null, null, null);
            return new ObjectResult(problem) { StatusCode = 500 };
        }
    }
}
