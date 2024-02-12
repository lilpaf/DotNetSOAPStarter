using DotNetSOAPStarter.ConsumerTestScripts.SOAP.Attributes;
using DotNetSOAPStarter.SOAP.Controllers;
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

        private readonly ILogger<Service1Controller> _logger;

        public Service2Controller(ILogger<Service1Controller> logger, IWebHostEnvironment env) : base(logger, env)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post(SOAP1_2RequestEnvelope envelope)
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
