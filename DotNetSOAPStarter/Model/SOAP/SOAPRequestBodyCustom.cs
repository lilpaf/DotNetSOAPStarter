using System.Xml.Serialization;

namespace DotNetSOAPStarter.Model.SOAP
{
    [XmlType(Namespace = DefaultNamespace)]
    public partial class SOAPRequestBodyCustom
    {
        public const string DefaultNamespacePrefix = "ser";
        public const string DefaultNamespace = "http://some.com/services/";
        public GetWeatherForecastRequest? GetWeatherForecast {  get; set; }
    }
}
