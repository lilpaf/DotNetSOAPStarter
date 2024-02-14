using DotNetSOAPStarter.SOAP.Model;
using DotNetSOAPStarter.Model.SOAP;
using DotNetSOAPStarter.SOAP;
using System.Xml.Serialization;
using static DotNetSOAPStarter.SOAP.Model.SOAP1_1Fault;
using static DotNetSOAPStarter.SOAP.Model.SOAP1_2Fault;


namespace SOAP.Model;

public partial class SOAP1_1ResponseBody : SOAPResponseBodyCustom
{
    [XmlElement(Namespace = SOAPConstants.SOAP1_1Namespace)]
    public SOAP1_1Fault? Fault { get; set; }
    public SOAP1_1Fault CreateFault(SOAP1_1FaultCodes code, string faultstring)
    {
        Fault = new SOAP1_1Fault(code, faultstring);
        return Fault;
    }
}

public partial class SOAP1_2ResponseBody : SOAPResponseBodyCustom
{
    [XmlElement(Namespace = SOAPConstants.SOAP1_2Namespace)]
    public SOAP1_2Fault? Fault { get; set; }

    public SOAP1_2Fault CreateFault(SOAP1_2FaultCodes code, Reason reason)
    {
        Fault = new SOAP1_2Fault(code, reason);
        return Fault;
    }

    public SOAP1_2Fault CreateFault(FaultCode code, Reason reason)
    {
        Fault = new SOAP1_2Fault(code, reason);
        return Fault;
    }

    public SOAP1_2Fault CreateFault(SOAP1_2FaultSubCodes code)
    {
        Fault = new SOAP1_2Fault(code);
        return Fault;
    }
}

[XmlInclude(typeof(SOAP1_1ResponseBody))]
[XmlInclude(typeof(SOAP1_2ResponseBody))]
public partial class SOAPResponseBody
{
}