namespace DotNetSOAPStarter.SOAP.Authentication.DataStores.File.Model
{
    public class PermissionItem
    {
        public PermissionItem(string service, IEnumerable<string>? allowedOperations, IEnumerable<string>? deniedOperations)
        {
            if (allowedOperations is not null && deniedOperations is not null)
            {
                throw new ArgumentException("PermissionItem cannot have both Allow and Deny collections.");
            }

            Service = service;
            AllowedOperations = allowedOperations;
            DeniedOperations = deniedOperations;
        }

        public string Service { get; set; }
        public IEnumerable<string>? AllowedOperations { get; }
        public IEnumerable<string>? DeniedOperations { get; }
    }
}
