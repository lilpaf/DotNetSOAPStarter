namespace DotNetSOAPStarter.SOAP.Model
{
    public class SOAPAuthData
    {
        public static string RequestKey_SOAPAuthData = "SOAPAuthData";

        public SOAPHeader? Header { get; set; }
        
        public string? RequestedSOAPOperationName { get; set; }
        
        // The Selected MVC Controller
        public string? TargetServiceName { get; set; }
        
        // Whether the ServiceName should be validated
        public bool PerformSOAPOperationAuthorization { get; set; } = true;
    }
}
