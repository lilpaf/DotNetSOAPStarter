using DotNetSOAPStarter.SOAP.Model;
using System.Xml.Serialization;

namespace DotNetSOAPStarter.Model.SOAP
{
    //Custom implementation on the SOAPRequestBody

    [XmlType(Namespace = DefaultNamespace)]
    public class SOAPRequestBodyCustom : SOAPRequestBody
    {
        public const string DefaultNamespacePrefix = "ser";
        public const string DefaultNamespace = "http://some.com/services/";
        
        public GetWeatherForecastRequest? GetWeatherForecast { get; set; }
    }
}

