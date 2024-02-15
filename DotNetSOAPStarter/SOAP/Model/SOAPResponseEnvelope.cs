using DotNetSOAPStarter.Model.SOAP;
using SOAP.Model;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace DotNetSOAPStarter.SOAP.Model
{
    [XmlRoot("Envelope", Namespace = SOAPConstants.SOAP1_1Namespace)]
    public class SOAP1_1ResponseEnvelope : SOAPResponseEnvelope 
    {
        public SOAP1_1ResponseEnvelope()
        {
            ns.Add(SOAPConstants.DefaultSOAPEnvelopeNamespacePrefix, SOAPConstants.SOAP1_1Namespace);
        }

        [NotNull]
        [XmlElement("Body")]
        public SOAP1_1ResponseBody? BodyTyped 
        {
            get
            {
                if (_body is null)
                    _body = (SOAP1_1ResponseBody)CreateBody();
                return (SOAP1_1ResponseBody)_body;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
        
    [XmlRoot("Envelope", Namespace = SOAPConstants.SOAP1_2Namespace)]
    public class SOAP1_2ResponseEnvelope : SOAPResponseEnvelope 
    {
        public SOAP1_2ResponseEnvelope()
        {
            ns.Add(SOAPConstants.DefaultSOAPEnvelopeNamespacePrefix, SOAPConstants.SOAP1_2Namespace);
        }

        [NotNull]
        [XmlElement("Body")]
        public SOAP1_2ResponseBody? BodyTyped
        {
            get
            {
                if (_body is null)
                    _body = (SOAP1_2ResponseBody)CreateBody();
                return (SOAP1_2ResponseBody)_body;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }

    public abstract class SOAPResponseEnvelope
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces ns = new();

        protected SOAPResponseBodyCustom? _body;

        [NotNull]
        [XmlIgnore]
        public SOAPResponseBodyCustom? Body
        {
            get
            {
                if (_body is null)
                    _body = CreateBody();

                return _body;
            }
            set { _body = value; }
        }
        
        protected virtual SOAPResponseBodyCustom CreateBody() 
            => this is SOAP1_1ResponseEnvelope ? new SOAP1_1ResponseBody() : new SOAP1_2ResponseBody();
    }
}
