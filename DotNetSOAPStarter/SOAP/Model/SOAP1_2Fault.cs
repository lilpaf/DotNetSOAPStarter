using DotNetSOAPStarter.SOAP;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using static DotNetSOAPStarter.SOAP.Model.SOAP1_2Fault;

namespace DotNetSOAPStarter.SOAP.Model
{
    [XmlType(Namespace = SOAPConstants.SOAP1_2Namespace)]
    public partial class SOAP1_2Fault : SOAPFault
    {
        public enum SOAP1_2FaultCodes
        {
            Sender,
            Receiver
        }

        public enum SOAP1_2FaultSubCodes
        {
            UnsupportedSecurityToken,
            UnsupportedAlgorithm,
            InvalidSecurity,
            InvalidSecurityToken,
            FailedAuthentication,
            FailedCheck,
            SecurityTokenUnavailable
        }

        public static Dictionary<SOAP1_2FaultSubCodes, string> SOAP1_2FaultSubCodesMessages = new()
        {
        {SOAP1_2FaultSubCodes.UnsupportedSecurityToken,"An unsupported token was provided"},
        {SOAP1_2FaultSubCodes.UnsupportedAlgorithm,"An unsupported signature or encryption algorithm was used"},
        {SOAP1_2FaultSubCodes.InvalidSecurity,"An error was discovered processing the <wsse:Security> header."},
        {SOAP1_2FaultSubCodes.InvalidSecurityToken,"An invalid security token was provided"},
        {SOAP1_2FaultSubCodes.FailedAuthentication,"The security token could not be authenticated or authorized"},
        {SOAP1_2FaultSubCodes.FailedCheck,"The signature or decryption was invalid"},
        {SOAP1_2FaultSubCodes.SecurityTokenUnavailable,"Referenced security token could not be retrieved"}
        };

        /* An example...
        <env:Code>
            <env:Value>env:Sender</env:Value>
        </env:Code>
        <env:Reason>
            <env:Text xml:lang="en-US">Message does not have necessary info</env:Text>
        </env:Reason>
        <env:Role>http://gizmos.com/order</env:Role>
        <env:Detail>
            <PO:order xmlns:PO="http://gizmos.com/orders/">Quantity element does not have a value</PO:order>
            <PO:confirmation xmlns:PO="http://gizmos.com/confirm">Incomplete address: no zip code</PO:confirmation>
        </env:Detail>
        */

        //Needed for serialization
        protected SOAP1_2Fault()
        {
            Code = new FaultCode(SOAP1_2FaultCodes.Receiver);
            this.Reason = new Reason("");
        }
        public SOAP1_2Fault(SOAP1_2FaultCodes faultCode, Reason reason)
        {
            Code = new FaultCode(faultCode);
            Reason = reason;
        }
        public SOAP1_2Fault(FaultCode faultCode, Reason reason)
        {
            Code = faultCode;
            Reason = reason;
        }
        public SOAP1_2Fault(SOAP1_2FaultSubCodes faultCode)
        {
            Code = new FaultCode(SOAP1_2FaultCodes.Sender, faultCode);
            Reason = new Reason(SOAP1_2FaultSubCodesMessages[faultCode]);
        }

        public FaultCode Code { get; set; }
        public Reason Reason { get; set; }

        [XmlIgnore]
        public Uri? Node { get; set; }

        [XmlElement("Node")]
        public string? NodeAsString
        {
            get
            {
                if (Node is not null)
                {
                    return Node.AbsoluteUri;
                }

                return null;
            }
            set
            {
                if (value is not null)
                {
                    Node = new Uri(value);
                    return;
                }

                Node = null;
            }
        }

        [XmlIgnore]
        public Uri? Role { get; set; }

        [XmlElement("Role")]
        public string? RoleAsString
        {
            get
            {
                if (Role is not null)
                {
                    return Role.AbsoluteUri;
                }

                return null;
            }
            set
            {
                if (value is not null)
                {
                    Role = new Uri(value);
                    return;
                }

                Role = null;
            }
        }

        public SOAPFaultDetail? Detail { get; set; }
    }
}

public class FaultCode
{
    public string Value { get; set; }
    
    public FaultCode? Subcode { get; set; }

    protected FaultCode()
    {
        Value = "";
    }

    public FaultCode(SOAP1_2FaultCodes faultCode)
    {
        this.Value = $"{SOAPConstants.DefaultSOAPEnvelopeNamespacePrefix}:{faultCode}";
    }
    
    public FaultCode(SOAP1_2FaultCodes faultCode, SOAP1_2FaultSubCodes faultSubcode) : this(faultCode)
    {
        this.Subcode = new FaultCode(faultSubcode);
    }
    
    public FaultCode(SOAP1_2FaultSubCodes faultCode)
    {
        this.Value = $"{SOAPConstants.DefaultSOAPSecurityNamespacePrefix}:{faultCode}";
    }
}

public class Reason
{
    [XmlElement("Text", Type = typeof(Text))]
    public Text[] Texts { get; set; }
    
    protected Reason()
    {
        this.Texts = new Text[] { new Text("") }; ;
    }
    
    public Reason(string text)
    {
        this.Texts = new Text[] { new Text(text) }; ;
    }
   
    public Reason(Text[] texts)
    {
        this.Texts = texts;
    }
}

public class Text
{
    [XmlIgnore]
    public CultureInfo Culture { get; set; }

    [XmlText]
    public string Value { get; set; }

    [XmlAttribute("lang", Namespace = "http://www.w3.org/XML/1998/namespace")]
    public string Lang
    {
        get 
        { 
            return Culture.Name; 
        }
        set 
        { 
            throw new NotImplementedException(); 
        }
    }

    protected Text()
    {
        Value = "";
        Culture = CultureInfo.CurrentCulture;
    }

    public Text(string value, CultureInfo? culture = null)
    {
        this.Culture = culture is null ? CultureInfo.CurrentCulture : culture;
        this.Value = value;
    }

}
