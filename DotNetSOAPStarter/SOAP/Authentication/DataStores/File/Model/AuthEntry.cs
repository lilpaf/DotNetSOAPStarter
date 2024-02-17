namespace DotNetSOAPStarter.SOAP.Authentication.DataStores.File.Model
{
    public class AuthEntry
    {
        public AuthEntry(string userId, string username, string password)
        {
            UserId = userId;
            Username = username;
            Password = password;
        }

        public string UserId { get; }
        public string Username { get; }
        public string Password { get; }
    }
}
