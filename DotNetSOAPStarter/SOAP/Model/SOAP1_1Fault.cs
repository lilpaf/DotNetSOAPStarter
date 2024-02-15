using DotNetSOAPStarter.Model.SOAP;
using System.Xml.Serialization;

namespace DotNetSOAPStarter.SOAP.Model
{
    [XmlType(Namespace = "")]
    public class SOAP1_1Fault : SOAPFault
    {
        [XmlIgnore]
        public SOAP1_1FaultCodes FaultCode { get; set; }

        [XmlElement("faultcode")]
        public string FaultCodeString 
        { 
            get 
            {
                return $"{SOAPConstants.DefaultSOAPEnvelopeNamespacePrefix}:{FaultCode}";
            } 
            set
            {
                throw new NotImplementedException();
            }
        }
        
        [XmlElement("faultstring")]
        public string FaultString { get; set; }

        [XmlElement("detail")]
        public SOAPFaultDetailCustom? Detail { get; set; }

        //Needed for serialization
        protected SOAP1_1Fault()
        {
            FaultString = "";
        }

        public SOAP1_1Fault(SOAP1_1FaultCodes faultCode, string faultString)
        {
            this.FaultCode = faultCode;
            this.FaultString = faultString;
        }

        public enum SOAP1_1FaultCodes
        {
            Client,
            Server
        }
    }
}
