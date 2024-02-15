using System.Xml.Serialization;

namespace DotNetSOAPStarter.SOAP.Model
{
    [XmlType(Namespace ="")]
    public class SOAPFaultDetail
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces? ns;
    }
}
