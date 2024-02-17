using System.Xml.Serialization;

namespace DotNetSOAPStarter.SOAP.Model
{
    public class SOAPHeader
    {
        [XmlElement(Namespace = SOAPConstants.SOAPSecurityNamespace)]
        public SOAPSecurity? Security { get; set; }
    }
}
