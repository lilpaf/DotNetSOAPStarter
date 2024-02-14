using DotNetSOAPStarter.Model.SOAP;
using DotNetSOAPStarter.SOAP.Attributes;
using DotNetSOAPStarter.SOAP.Controllers;
using DotNetSOAPStarter.SOAP.Model;
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

        [HttpPost]
        [Produces("application/xml")]
        public IActionResult Post(SOAP1_1RequestEnvelope envelope)
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
