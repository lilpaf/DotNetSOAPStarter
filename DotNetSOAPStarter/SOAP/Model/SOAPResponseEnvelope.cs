using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace DotNetSOAPStarter.SOAP.Model
{
    [XmlRoot("Envelope", Namespace = SOAPConstants.Soap1_1Namespace)]
    public partial class SOAP1_1ResponseEnvelope : SOAPResponseEnvelope { }
        
    [XmlRoot("Envelope", Namespace = SOAPConstants.Soap1_2Namespace)]
    public partial class SOAP1_2ResponseEnvelope : SOAPResponseEnvelope { }

    public abstract partial class SOAPResponseEnvelope
    {
        protected SOAPResponseBody? _body;

        [NotNull]
        public SOAPResponseBody? Body
        {
            get
            {
                if (_body is null)
                    _body = new SOAPResponseBody();

                return _body;
            }
            set { _body = value; }
        }
    }
}
