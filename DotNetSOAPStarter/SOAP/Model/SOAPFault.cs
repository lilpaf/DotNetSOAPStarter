using System.Xml.Serialization;

namespace DotNetSOAPStarter.SOAP.Model
{
    [XmlType(Namespace = "")]
    public abstract class SOAPFault
    {
        public enum PartyAtFault
        {
            Client,
            Server
        }
    }
}
