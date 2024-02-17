using DotNetSOAPStarter.Model;
using DotNetSOAPStarter.Model.SOAP;
using DotNetSOAPStarter.SOAP.Attributes;
using DotNetSOAPStarter.SOAP.Controllers;
using DotNetSOAPStarter.SOAP.Filters;
using DotNetSOAPStarter.SOAP.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetSOAPStarter.Controllers
{
    [SOAPController(SOAPVersion.v1_1)]
    public class Service1Controller : SOAPControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<Service1Controller> _logger;

        public Service1Controller(ILogger<Service1Controller> logger, IWebHostEnvironment env) : base(logger, env)
        {
            _logger = logger;
        }

        public override SOAPResponseEnvelope CreateSOAPResponseEnvelope()
        {
            var envelope =  base.CreateSOAPResponseEnvelope();
            envelope.ns.Add(SOAPResponseBodyCustom.DefaultNamespacePrefix, SOAPResponseBodyCustom.DefaultNamespace);
            return envelope;
        }

        [Authorize]
        [HttpPost]
        [PayloadRequired]
        [Consumes("application/xml")]
        public IActionResult OperationSelector(SOAPRequestEnvelope envelope)
        {
            var user = HttpContext.User;

            if (envelope.Body?.GetWeatherForecast is not null) 
            {
                return GetWeatherForecast(envelope.Body.GetWeatherForecast);
            }

            return SOAPOperationNotFound();
        }

        private IActionResult GetWeatherForecast(GetWeatherForecastRequest request)
        {
            //Fake error scenario
            if (request.Value == 10)
            {
                var errorList = new List<Error>
                {
                    new Error() { Message = "some error message" },
                    new Error() { Message = "some error message" },
                    new InputValidationError() { FieldName = "testfield", Message = "some validation error" },
                    new BusinessRuleError() { RuleName = "rulename", Message = "some business rule error" }
                };

                return SOAPFault("an error occurred", detail: new SOAPFaultDetailCustom()
                {
                    Errors = errorList.ToArray(),
                    Messages = ["Hello"],
                });
            }

            SOAPResponseEnvelope response = CreateSOAPResponseEnvelope();
            response.Body.GetWeatherForecastResponse = new()
            {
                WeatherForecasts = Enumerable.Range(1, 5)
                .Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray()
            };

            return Ok(response);
        }
    }
}
