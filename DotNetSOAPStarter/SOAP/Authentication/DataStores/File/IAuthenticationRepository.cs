using System.Security.Claims;

namespace DotNetSOAPStarter.SOAP.Authentication.DataStores.File
{
    public interface IAuthenticationRepository
    {
        Task<bool> AuthenticateUserAsync(ClaimsPrincipal claims, object? authData);
    }
}
