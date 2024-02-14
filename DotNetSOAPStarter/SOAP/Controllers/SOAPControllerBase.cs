using DotNetSOAPStarter.SOAP.Attributes;
using DotNetSOAPStarter.SOAP.Model;
using Microsoft.AspNetCore.Mvc;
using SOAP;
using System.Text;
using static DotNetSOAPStarter.SOAP.Model.SOAPFault;

namespace DotNetSOAPStarter.SOAP.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public abstract class SOAPControllerBase : ControllerBase
    {
        private readonly ILogger<SOAPControllerBase> _logger;
        private readonly IWebHostEnvironment _env;
        protected SOAPVersion SOAPVersion { get; init; }

        public SOAPControllerBase(ILogger<SOAPControllerBase> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;

            SOAPControllerAttribute? soapVersionAttribute = Attribute.GetCustomAttribute(GetType(), typeof(SOAPControllerAttribute)) as SOAPControllerAttribute;

            if (soapVersionAttribute is null)
                throw new Exception("class deriving from SOAPControllerBase is missing the SOAPController attribute");

            SOAPVersion = soapVersionAttribute.SOAPVersions;
        }

        public virtual SOAPResponseEnvelope CreateSOAPResponseEnvelope()
            => SOAPVersion == SOAPVersion.v1_1 ? new SOAP1_1ResponseEnvelope() : new SOAP1_2ResponseEnvelope();

        #region WSDL Handling

        [HttpGet]
        public async Task<IActionResult> Get(string? wsdl, string? xsd)
        {
            string controllerName = ControllerContext.RouteData.Values["controller"]?.ToString() ?? string.Empty;

            if (!string.IsNullOrEmpty(controllerName))
            {
                controllerName += '/';
            }

            if (wsdl == string.Empty)
            {
                wsdl = "wsdl";
            }

            if (wsdl is not null)
            {
                return await ProcessWsdlFile($"~/wsdl/{controllerName}{wsdl}.xml");
            }

            if (xsd is not null)
            {
                if (xsd == string.Empty)
                {
                    return SOAPFault("xsd parameter can not be empty");
                }

                return await ProcessWsdlFile($"~/wsdl/{controllerName}{xsd}.xml");
            }

            return SOAPFault("invalid request.");
        }

        protected async Task<IActionResult> ProcessWsdlFile(string path)
        {
            var _baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";

            if (path.StartsWith("~"))
            {
                path = path.Replace("~", _env.ContentRootPath);
            }
            string content;

            try
            {
                content = await System.IO.File.ReadAllTextAsync(path, Encoding.UTF8);
            }
            catch (DirectoryNotFoundException)
            {
                return SOAPFault("wsdl directory not found");
            }
            catch (FileNotFoundException)
            {
                return SOAPFault("wsdl file not found");
            }

            // Replace placeholders with actual values 
            content = content.Replace("{SERVICEURL}", _baseUrl);
            return File(Encoding.UTF8.GetBytes(content), "text/xml;charset=UTF-8");
        }

        #endregion

        #region Faults

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public ObjectResult SOAPFault(string faultstring, SOAPFaultDetail? detail = null, PartyAtFault faultcode = PartyAtFault.Server, Uri? node = null, Uri? role = null)
        {
            if (SOAPVersion == SOAPVersion.v1_1)
                // discard node and role 
                return SOAPEnvelopeResponses.SOAPFault((SOAP1_1ResponseEnvelope)CreateSOAPResponseEnvelope(), faultstring, detail, faultcode);
            else
                return SOAPEnvelopeResponses.SOAPFault((SOAP1_2ResponseEnvelope)CreateSOAPResponseEnvelope(), new Reason(faultstring), node, role, detail, faultcode);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public ObjectResult SOAPFault(Reason reason, SOAPFaultDetail? detail = null, PartyAtFault faultcode = PartyAtFault.Server, Uri? node = null, Uri? role = null)
        {
            if (SOAPVersion == SOAPVersion.v1_1)
                // get the faultstring from the 'reason' and discard node and role 
                return SOAPEnvelopeResponses.SOAPFault((SOAP1_1ResponseEnvelope)CreateSOAPResponseEnvelope(), reason.Texts[0].Value, detail, faultcode);
            else
                return SOAPEnvelopeResponses.SOAPFault((SOAP1_2ResponseEnvelope)CreateSOAPResponseEnvelope(), reason, node, role, detail, faultcode);
        }

        #endregion
    }
}
