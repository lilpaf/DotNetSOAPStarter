using System.Xml.Serialization;

namespace DotNetSOAPStarter.SOAP.Model
{
    [XmlType(Namespace = SOAPRequestBody.DefaultNamespace)]
    public partial class SOAPRequestBody
    {
        public const string DefaultNamespacePrefix = "ser";
        public const string DefaultNamespace = "http://some.com/services/";
        public GetWeatherForecastRequest? GetWeatherForecast {  get; set; }
    }
}
