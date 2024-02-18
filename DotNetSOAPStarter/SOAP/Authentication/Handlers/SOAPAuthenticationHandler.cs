using DotNetSOAPStarter.SOAP.Attributes;
using DotNetSOAPStarter.SOAP.Authentication.DataStores.File;
using DotNetSOAPStarter.SOAP.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SOAP;
using static DotNetSOAPStarter.SOAP.Model.SOAP1_2Fault;
using static DotNetSOAPStarter.SOAP.Model.SOAPFault;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Xml.Serialization;
using System.Xml;

namespace DotNetSOAPStarter.SOAP.Authentication.Handlers
{
    public class SOAPAuthenticationHandler : AuthenticationHandler<SOAPAuthenticationHandlerOptions>
    {
        readonly IAuthenticationRepository _authNRepository;
        protected string? authErrorMessage;
        SOAP1_2FaultSubCodes? authErrorCode;

        public SOAPAuthenticationHandler(IOptionsMonitor<SOAPAuthenticationHandlerOptions> options, ILoggerFactory loggerFactory, UrlEncoder encoder, ISystemClock clock, IAuthenticationRepository authNRepository) : base(options, loggerFactory, encoder, clock)
        {
            _authNRepository = authNRepository;
        }

        protected async override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            SOAPControllerAttribute? soapAttribute = Context.GetEndpoint()?
                .Metadata.GetMetadata<SOAPControllerAttribute>();

            if (soapAttribute is not null)
            {
                if (authErrorMessage is not null)
                {
                    await Context.WriteResultAsync(SOAPEnvelopeResponses.SOAPFault(
                        soapAttribute.SOAPVersion,
                        authErrorMessage,
                        faultcode: PartyAtFault.Client));
                }
                else
                {
                    await Context.WriteResultAsync(SOAPEnvelopeResponses.SOAPFault(
                        soapAttribute.SOAPVersion,
                        authErrorCode is not null ? authErrorCode.Value : SOAP1_2FaultSubCodes.InvalidSecurity));
                }

                return;
            }

            await base.HandleChallengeAsync(properties);
        }

        protected async override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            SOAPControllerAttribute? soapAttribute = Context.GetEndpoint()?.Metadata.GetMetadata<SOAPControllerAttribute>();

            if (soapAttribute is not null)
            {
                await Context.WriteResultAsync(SOAPEnvelopeResponses.SOAPFault(soapAttribute.SOAPVersion, authErrorCode is not null ? authErrorCode.Value : SOAP1_2FaultSubCodes.FailedAuthentication));
            
                return;
            }

            await base.HandleForbiddenAsync(properties);
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            SOAPControllerAttribute? soapAttribute = Context.GetEndpoint()?
                .Metadata.GetMetadata<SOAPControllerAttribute>();

            // Not calling a SOAP Controller
            if (soapAttribute is null)
            {
                return AuthenticateResult.NoResult();
            }

            HttpRequest request = Context.Request;
            var readResult = await ReadAuthenticationDataAsync(request, soapAttribute);
            SOAPAuthData? authData = readResult.authData;
            string? errorMsg = readResult.errMsg;

            if (errorMsg is not null)
            {
                authErrorMessage = errorMsg;
                return AuthenticateResult.Fail(authErrorMessage);
            }

            // Validation
            if (authData is null)
            {
                authErrorMessage = "SOAP Header is empty or incorrect type";
                return AuthenticateResult.Fail(authErrorMessage);
            }

            if (authData?.Header?.Security?.UsernameToken?.Username is null)
            {
                authErrorCode = SOAP1_2FaultSubCodes.InvalidSecurityToken;
                return AuthenticateResult.Fail("SOAP Security UsernameToken Username is missing");
            }

            if (authData?.Header?.Security?.UsernameToken?.Password is null)
            {
                authErrorCode = SOAP1_2FaultSubCodes.InvalidSecurityToken;
                return AuthenticateResult.Fail("SOAP Security UsernameToken Password is missing");
            }

            if (string.IsNullOrEmpty(authData?.RequestedSOAPOperationName))
            {
                authErrorMessage = "SOAPOperationName was not found";
                return AuthenticateResult.Fail(authErrorMessage);
            }

            // Create the principal & identity
            ClaimsPrincipal principal = new ClaimsPrincipal();
            ClaimsIdentity identity = new ClaimsIdentity(SOAPAuthenticationDefaults.AuthenticationScheme + "Identity");
            principal.AddIdentity(identity);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, authData.Header.Security.UsernameToken.Username));

            // Authenticate the user
            if (await _authNRepository.AuthenticateUserAsync(
                principal,
                authData.Header.Security.UsernameToken.Password?.Value))
            {
                // Add the header into the request context so Auth Handlers can use it.
                Context.Items.Add(SOAPAuthData.RequestKey_SOAPAuthData, authData);
                AuthenticationTicket ticket = new AuthenticationTicket(
                    principal,
                    SOAPAuthenticationDefaults.AuthenticationScheme);
                return AuthenticateResult.Success(ticket);
            }

            authErrorCode = SOAP1_2FaultSubCodes.FailedAuthentication;
            return AuthenticateResult.Fail("Authentication Failed");
        }

        protected virtual async Task<(SOAPAuthData? authData, string? errMsg)> ReadAuthenticationDataAsync(
            HttpRequest request,
            SOAPControllerAttribute soapAttribute)
        {
            request.EnableBuffering();
            request.Body.Position = 0;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Async = true;
            settings.IgnoreWhitespace = true;

            using XmlReader reader = XmlReader.Create(request.Body, settings);
            string soapNamespace = soapAttribute.SOAPVersion == SOAPVersion.v1_1 ? SOAPConstants.SOAP1_1Namespace : SOAPConstants.SOAP1_2Namespace;
            SOAPAuthData? authData = new SOAPAuthData();
            string? errorMsg = null;

            try
            {
                while (await reader.ReadAsync())
                {
                    if (reader.IsStartElement() && reader.LocalName == "Envelope" && reader.NamespaceURI == soapNamespace)
                    {
                        bool bBodyFound = false;
                        bool bHeaderFound = false;
                        bool bDataRead = await reader.ReadAsync();

                        while (bDataRead && (bBodyFound == false || bHeaderFound == false))
                        {
                            if (reader.IsStartElement() && reader.LocalName == "Body" && reader.NamespaceURI == soapNamespace)
                            {
                                bBodyFound = true;
                                reader.ReadStartElement();
                                authData.RequestedSOAPOperationName = reader.LocalName;
                                await reader.SkipAsync();
                            }

                            if (reader.IsStartElement() && reader.LocalName == "Header" && reader.NamespaceURI == soapNamespace)
                            {
                                bHeaderFound = true;
                                XmlRootAttribute root = new XmlRootAttribute("Header");
                                XmlSerializer serializer = new XmlSerializer(typeof(SOAPHeader), null, null, root, soapNamespace);
                                authData.Header = serializer.Deserialize(reader) as SOAPHeader;
                                continue;
                            }

                            bDataRead = await reader.ReadAsync();
                        }

                        if (bBodyFound == false || bHeaderFound == false)
                        {
                            authData = null;
                            errorMsg = "Payload is missing a Header or a Body";
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                authData = null;
                errorMsg = ex.Message;

                // Makes the error consistent with other missing payload error scenarios
                if (request.Body.Position == 0)
                {
                    errorMsg = "Request is missing a payload";
                }
            }

            // Reset the stream position so Model Binding can re-read the stream
            request.Body.Position = 0;
            return (authData, errorMsg);
        }
    }

    public static class HttpResponseExtension
    {
        private static readonly RouteData EmptyRouteData = new RouteData();
        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();
        public static Task WriteResultAsync(this HttpContext context, ObjectResult result)
        {
            var executor = context.RequestServices.GetRequiredService<IActionResultExecutor<ObjectResult>>();
            var routeData = context.GetRouteData() ?? EmptyRouteData;
            var actionContext = new ActionContext(context, routeData, EmptyActionDescriptor);
            return executor.ExecuteAsync(actionContext, result);
        }
    }
}
