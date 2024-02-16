using DotNetSOAPStarter.SOAP.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace DotNetSOAPStarter.SOAP.Attributes
{
    public class SOAPControllerAttribute : ProducesAttribute
    {
        public SOAPControllerAttribute(SOAPVersion soapVersions) : base(MediaTypeNames.Application.Xml)
        {
            SOAPVersion = soapVersions;
        }

        public SOAPVersion SOAPVersion { get; }
    }
}
