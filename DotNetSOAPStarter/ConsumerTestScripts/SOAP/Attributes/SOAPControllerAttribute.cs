using DotNetSOAPStarter.SOAP.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace DotNetSOAPStarter.ConsumerTestScripts.SOAP.Attributes
{
    public class SOAPControllerAttribute : ProducesAttribute
    {
        public SOAPControllerAttribute(SOAPVersion soapVersions) : base(MediaTypeNames.Application.Xml)
        {
            SOAPVersions = soapVersions;
        }

        public SOAPVersion SOAPVersions { get; }
    }
}
