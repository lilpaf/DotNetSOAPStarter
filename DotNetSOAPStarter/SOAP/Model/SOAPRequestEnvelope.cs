﻿using DotNetSOAPStarter.Model.SOAP;
using System.Xml.Serialization;

namespace DotNetSOAPStarter.SOAP.Model
{
    [XmlRoot("Envelope", Namespace = SOAPConstants.SOAP1_1Namespace)]
    public class SOAP1_1RequestEnvelope : SOAPRequestEnvelope { }
    
    [XmlRoot("Envelope", Namespace = SOAPConstants.SOAP1_2Namespace)]
    public class SOAP1_2RequestEnvelope : SOAPRequestEnvelope { }

    public abstract class SOAPRequestEnvelope
    {
        public SOAPHeader? Header { get; set; }
        public SOAPRequestBodyCustom? Body { get; set; }
    }
}
