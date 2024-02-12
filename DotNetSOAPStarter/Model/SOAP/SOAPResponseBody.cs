using System.Xml.Serialization;

namespace DotNetSOAPStarter.SOAP.Model
{
    [XmlType(Namespace = SOAPResponseBody.DefaultNamespace)]
    public partial class SOAPResponseBody
    {
        public const string DefaultNamespacePrefix = "ser";
        public const string DefaultNamespace = "http://some.com/services/";
        public GetWeatherForecastResponse? GetWeatherForecastResponse { get; set; }
    }
}
