using DotNetSOAPStarter.ConsumerTestScripts.SOAP.Attributes;
using DotNetSOAPStarter.SOAP.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text;

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

            //ToDo string.NullOrEmpty()
            if (wsdl is not null)
            {
                return await ProcessWsdlFile($"~/wsdl/{controllerName}{wsdl}.xml");
            }
            //ToDo string.NullOrEmpty()
            if (xsd is not null)
            {
                if (xsd == string.Empty)
                {
                    //ToDo should be SOAP fault
                    return BadRequest("xsd parameter can not be empty");
                }

                return await ProcessWsdlFile($"~/wsdl/{controllerName}{xsd}.xml");
            }

            //ToDo should be SOAP fault
            return BadRequest("invalid request.");
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
                //ToDo should be SOAP fault
                return new ObjectResult("wsdl directory not found") { StatusCode = StatusCodes.Status500InternalServerError };
            }
            catch (FileNotFoundException)
            {
                //ToDo should be SOAP fault
                return new ObjectResult("wsdl file not found") { StatusCode = StatusCodes.Status500InternalServerError };
            }

            // Replace placeholders with actual values 
            content = content.Replace("{SERVICEURL}", _baseUrl);
            return File(Encoding.UTF8.GetBytes(content), "text/xml;charset=UTF-8");
        }

        #endregion

    }
}
