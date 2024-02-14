using System.Xml.Serialization;

namespace DotNetSOAPStarter.Model.SOAP
{
    [XmlType(Namespace = DefaultNamespace)]
    public partial class SOAPResponseBodyCustom
    {
        public const string DefaultNamespacePrefix = "ser";
        public const string DefaultNamespace = "http://some.com/services/";
        public GetWeatherForecastResponse? GetWeatherForecastResponse { get; set; }
    }
}
