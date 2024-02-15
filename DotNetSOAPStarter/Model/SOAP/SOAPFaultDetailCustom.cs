using DotNetSOAPStarter.SOAP.Model;
using System.Xml.Serialization;

namespace DotNetSOAPStarter.Model.SOAP
{
    // Custom extended implementation of the SOAPFaultDetail
    public class SOAPFaultDetailCustom : SOAPFaultDetail
    {
        public Error[]? Errors { get; set; }

        [XmlArrayItem("message")]
        public string[]? Messages { get; set; }
    }

    [XmlInclude(typeof(InputValidationError))]
    [XmlInclude(typeof(BusinessRuleError))]
    public class Error
    {
        public string? Message { get; set; }
    }

    public class InputValidationError : Error
    {
        public string? FieldName { get; set; }
    }

    public class BusinessRuleError : Error
    {
        public string? RuleName { get; set; }
    }
}
