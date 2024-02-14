using DotNetSOAPStarter.SOAP.Model;
using Microsoft.AspNetCore.Mvc;
using static DotNetSOAPStarter.SOAP.Model.SOAP1_1Fault;
using static DotNetSOAPStarter.SOAP.Model.SOAP1_2Fault;
using static DotNetSOAPStarter.SOAP.Model.SOAPFault;

namespace SOAP
{
    public static class SOAPEnvelopeResponses
    {
        public static ObjectResult SOAPFault(SOAPVersion version, string faultstring = "", SOAPFaultDetail? detail = null, PartyAtFault? faultcode = PartyAtFault.Server)
        {
            if (version == SOAPVersion.v1_1)
            {
                SOAP1_1ResponseEnvelope env = new SOAP1_1ResponseEnvelope();
                return SOAPFault(env, faultstring, detail, faultcode);
            }
            else
            {
                SOAP1_2ResponseEnvelope env = new SOAP1_2ResponseEnvelope();
                return SOAPFault(env, new Reason(faultstring), null, null, detail, faultcode);
            }
        }

        public static ObjectResult SOAPFault(SOAPVersion version, SOAP1_2FaultSubCodes faultcode, Uri? node = null, Uri? role = null, SOAPFaultDetail? detail = null)
        {
            if (version == SOAPVersion.v1_1)
            {
                SOAP1_1ResponseEnvelope env = new SOAP1_1ResponseEnvelope();
                return SOAPFault(env, SOAP1_2Fault.SOAP1_2FaultSubCodesMessages[faultcode], detail, PartyAtFault.Client);
            }
            else
            {
                SOAP1_2ResponseEnvelope env = new SOAP1_2ResponseEnvelope();
                return SOAPFault(env, faultcode, node, role, detail);
            }
        }

        public static ObjectResult SOAPFault(SOAP1_1ResponseEnvelope env, string faultstring = "", SOAPFaultDetail? detail = null, PartyAtFault? faultcode = PartyAtFault.Server)
        {
            var fault = env.BodyTyped.CreateFault(faultcode == PartyAtFault.Client ? SOAP1_1FaultCodes.Client : SOAP1_1FaultCodes.Server, faultstring);
            fault.Detail = detail;
            return SOAPFaultResult(env);
        }

        public static ObjectResult SOAPFault(SOAP1_2ResponseEnvelope env, Reason reason, Uri? node = null, Uri? role = null, SOAPFaultDetail? detail = null, PartyAtFault? faultcode = PartyAtFault.Server)
        {
            var fault = env.BodyTyped.CreateFault(faultcode == PartyAtFault.Client ? SOAP1_2FaultCodes.Sender : SOAP1_2FaultCodes.Receiver, reason);
            fault.Node = node;
            fault.Role = role;
            fault.Detail = detail;
            return SOAPFaultResult(env);
        }

        public static ObjectResult SOAPFault(SOAP1_2ResponseEnvelope env, Reason reason, SOAP1_2FaultCodes faultcode, Uri? node = null, Uri? role = null, SOAPFaultDetail? detail = null)
        {
            var fault = env.BodyTyped.CreateFault(faultcode, reason);
            fault.Node = node;
            fault.Role = role;
            fault.Detail = detail;
            return SOAPFaultResult(env);
        }

        public static ObjectResult SOAPFault(SOAP1_2ResponseEnvelope env, Reason reason, FaultCode faultcode, Uri? node = null, Uri? role = null, SOAPFaultDetail? detail = null)
        {
            var fault = env.BodyTyped.CreateFault(faultcode, reason);
            fault.Node = node;
            fault.Role = role;
            fault.Detail = detail;
            return SOAPFaultResult(env);
        }
        public static ObjectResult SOAPFault(SOAP1_2ResponseEnvelope env, SOAP1_2FaultSubCodes faultcode, Uri? node = null, Uri? role = null, SOAPFaultDetail? detail = null)
        {
            var fault = env.BodyTyped.CreateFault(faultcode);
            fault.Node = node;
            fault.Role = role;
            fault.Detail = detail;
            return SOAPFaultResult(env);
        }
        public static ObjectResult SOAPFaultResult(SOAPResponseEnvelope env)
        {
            var result = new ObjectResult(env) { StatusCode = StatusCodes.Status500InternalServerError };
            result.ContentTypes.Add(System.Net.Mime.MediaTypeNames.Application.Xml);
            return result;
        }
    }
}