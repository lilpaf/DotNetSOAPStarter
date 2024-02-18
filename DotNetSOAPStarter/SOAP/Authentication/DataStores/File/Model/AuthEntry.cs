namespace DotNetSOAPStarter.SOAP.Authentication.DataStores.File.Model
{
    public class AuthEntry
    {
        public AuthEntry(string userId, string username, string password, IEnumerable<string>? roles)
        {
            UserId = userId;
            Username = username;
            Password = password;
            Roles = roles;
        }

        public string UserId { get; }
        public string Username { get; }
        public string Password { get; }
        public IEnumerable<String>? Roles { get; }
    }
}
