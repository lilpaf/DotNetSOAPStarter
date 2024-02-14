using System.Xml.Serialization;

namespace DotNetSOAPStarter.SOAP.Model
{
    [XmlRoot("Envelope", Namespace = SOAPConstants.SOAP1_1Namespace)]
    public partial class SOAP1_1RequestEnvelope : SOAPRequestEnvelope { }
    
    [XmlRoot("Envelope", Namespace = SOAPConstants.SOAP1_2Namespace)]
    public partial class SOAP1_2RequestEnvelope : SOAPRequestEnvelope { }

    public abstract partial class SOAPRequestEnvelope
    {
        public SOAPHeader? Header { get; set; }
        public SOAPRequestBody? Body { get; set; }
    }
}
