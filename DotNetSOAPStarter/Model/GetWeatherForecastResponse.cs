using System.Xml.Serialization;

namespace DotNetSOAPStarter.Model
{
    //[XmlType(Namespace = "")]
    public class GetWeatherForecastResponse
    {
        //[XmlNamespaceDeclarations]
        //public XmlSerializerNamespaces ns = new();

        //public GetWeatherForecastResponse()
        //{
        //    ns.Add(SOAPResponseBody.DefaultNamespacePrefix, SOAPResponseBody.DefaultNamespace);
        //}

        public WeatherForecast[]? WeatherForecasts { get; set; }
    }
}