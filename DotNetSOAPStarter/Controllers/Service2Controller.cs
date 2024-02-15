using DotNetSOAPStarter.Model;
using DotNetSOAPStarter.Model.SOAP;
using DotNetSOAPStarter.SOAP.Attributes;
using DotNetSOAPStarter.SOAP.Controllers;
using DotNetSOAPStarter.SOAP.Filters;
using DotNetSOAPStarter.SOAP.Model;
using Microsoft.AspNetCore.Mvc;

namespace DotNetSOAPStarter.Controllers
{
    [SOAPController(SOAPVersion.v1_2)]
    public class Service2Controller : SOAPControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<Service2Controller> _logger;

        public Service2Controller(ILogger<Service2Controller> logger, IWebHostEnvironment env) : base(logger, env)
        {
            _logger = logger;
        }

        public override SOAPResponseEnvelope CreateSOAPResponseEnvelope()
        {
            var envelope = base.CreateSOAPResponseEnvelope();
            envelope.ns.Add(SOAPResponseBodyCustom.DefaultNamespacePrefix, SOAPResponseBodyCustom.DefaultNamespace);
            return envelope;
        }

        [HttpPost]
        [PayloadRequired]
        [Consumes("application/xml")]
        public IActionResult OperationSelector(SOAP1_2RequestEnvelope envelope)
        {
            if (envelope.Body?.GetWeatherForecast is not null)
            {
                return GetWeatherForecast(envelope.Body?.GetWeatherForecast);
            }

            return SOAPOperationNotFound();
        }

        
        private IActionResult GetWeatherForecast(GetWeatherForecastRequest request)
        {
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
