using SOAP.Model;
using System.Xml.Serialization;

namespace DotNetSOAPStarter.Model.SOAP
{
    //Custom implementation on the SOAPResponseBody
    [XmlType(Namespace = DefaultNamespace)]
    public class SOAPResponseBodyCustom : SOAPResponseBody
    {
        public const string DefaultNamespacePrefix = "ser";
        public const string DefaultNamespace = "http://some.com/services/";
        public GetWeatherForecastResponse? GetWeatherForecastResponse { get; set; }
    }
}
