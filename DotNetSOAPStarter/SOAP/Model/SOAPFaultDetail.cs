using System.Xml.Serialization;

namespace DotNetSOAPStarter.SOAP.Model
{
    [XmlType(Namespace ="")]
    public partial class SOAPFaultDetail
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces? ns;
    }
}
