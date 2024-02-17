namespace DotNetSOAPStarter.SOAP.Authentication.DataStores.File
{
    public interface IPasswordEncoder
    {
        string Encode(string rawPassword);
        (bool matches, bool needsUpgrade) Matches(string rawPassword, string encodedPassword);
    }
}
