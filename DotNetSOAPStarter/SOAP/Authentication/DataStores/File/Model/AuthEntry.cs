namespace DotNetSOAPStarter.SOAP.Authentication.DataStores.File.Model
{
    public class AuthEntry
    {
        public AuthEntry(
            string userId, 
            string username,
            string password,
            IEnumerable<string>? roles, 
            IEnumerable<PermissionItem>? allowedPermissions, 
            IEnumerable<PermissionItem>? deniedPermissions)
        {
            UserId = userId;
            Username = username;
            Password = password;
            Roles = roles;
            AllowedPermissions = allowedPermissions;
            DeniedPermissions = deniedPermissions;
        }

        public string UserId { get; }
        public string Username { get; }
        public string Password { get; }
        public IEnumerable<string>? Roles { get; }
        public IEnumerable<PermissionItem>? AllowedPermissions { get; }
        public IEnumerable<PermissionItem>? DeniedPermissions { get; }
    }
}
