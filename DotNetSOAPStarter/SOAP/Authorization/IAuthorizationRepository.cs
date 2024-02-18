using System.Security.Claims;

namespace DotNetSOAPStarter.SOAP.Authorization
{
    public interface IAuthorizationRepository
    {
        Task<bool> AuthorizeUserAsync(ClaimsPrincipal principal, object? authData);
    }
}
