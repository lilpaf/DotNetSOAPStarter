using DotNetSOAPStarter.SOAP.Authentication.DataStores.File.Model;
using DotNetSOAPStarter.SOAP.Authorization;
using DotNetSOAPStarter.SOAP.Model;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;

namespace DotNetSOAPStarter.SOAP.Authentication.DataStores.File
{
    public class FileAuthDataStore : IAuthenticationRepository, IAuthorizationRepository
    {
        private readonly ILogger _logger;
        protected const string DefaultAuthDataStoreFolderName = "SOAP/Authentication/SOAPAuthentication/DataStore/File";
        protected const string DefaultAuthDataStoreFileName = "AuthDataStore.json";
        protected const string AuthDataStoreFileNameConfigKey = "SOAPAuthenticationDataStore:FilePath";
        protected IDictionary<string, AuthEntry> users = new Dictionary<string, AuthEntry>();
        protected IPasswordEncoder _passwordEncoder;
        public FileAuthDataStore(IConfiguration configuration, ILogger<FileAuthDataStore> logger, IPasswordEncoder passwordEncoder)
        {
            _logger = logger;
            _passwordEncoder = passwordEncoder;
            string? filePath = configuration[AuthDataStoreFileNameConfigKey];
            
            if (filePath is null)
            {
                string? baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                
                if (baseDir is null)
                {
                    throw new FileNotFoundException("FileAuthDataStore cannot find the Auth Information File Directory");
                }

                string[] files = Directory.GetFiles(baseDir, DefaultAuthDataStoreFileName, SearchOption.TopDirectoryOnly);
                
                if (files.Length != 1)
                {
                    files = Directory.GetFiles(baseDir, DefaultAuthDataStoreFileName, SearchOption.AllDirectories);
                    
                    if (files.Length == 0)
                    {
                        throw new FileNotFoundException("FileAuthDataStore cannot find the Auth Information File");
                    }

                    if (files.Length > 1)
                    {
                        throw new FileNotFoundException("FileAuthDataStore found more than one Auth Information File, there can be only one!");
                    }
                }

                filePath = files[0];
            }
            
            string jsonString = System.IO.File.ReadAllText(filePath);
            
            using (JsonDocument document = JsonDocument.Parse(jsonString))
            {
                foreach (JsonElement userElement in document.RootElement.EnumerateArray())
                {
                    string userId = userElement.GetMandatoryString("UserID");
                    string username = userElement.GetMandatoryString("Username");
                    string password = userElement.GetMandatoryString("Password");

                    List<string>? roles = null;

                    if (userElement.TryGetProperty("Roles", out JsonElement rolesColl))
                    {
                        roles = new List<string>();
                        foreach (JsonElement roleElement in rolesColl.EnumerateArray())
                        {
                            string? role = roleElement.GetString();
                            if (role is not null)
                                roles.Add(role);
                        }
                    }

                    List<PermissionItem>? allowedPermissions = ReadPermissionCollection(userElement, "Allow");
                    List<PermissionItem>? deniedPermissions = ReadPermissionCollection(userElement, "Deny");

                    AuthEntry user = new AuthEntry(userId, username, password, roles, allowedPermissions, deniedPermissions);
                    users.Add(new KeyValuePair<string, AuthEntry>(user.UserId, user));
                }
            }

            if (users.Count == 0)
            {
                _logger.LogWarning("No users added to Auth DataStore. This might break the Authentication mechanism.");
            }
        }

        protected List<PermissionItem>? ReadPermissionCollection(JsonElement parent, string permissionPropertyName)
        {
            List<PermissionItem>? permissions = null;

            if (parent.TryGetProperty(permissionPropertyName, out JsonElement permissionsColl))
            {
                permissions = new List<PermissionItem>();
                
                foreach (JsonElement serviceElement in permissionsColl.EnumerateArray())
                {
                    var enumerator = serviceElement.EnumerateObject();
                    
                    if (enumerator.Any())
                    {
                        JsonProperty serviceObject = enumerator.First();
                        List<string>? allowedOperations = ReadOperationCollection(serviceObject.Value, "Allow");
                        List<string>? deniedOperations = ReadOperationCollection(serviceObject.Value, "Deny");
                        string service = serviceObject.Name;
                        permissions.Add(new PermissionItem(service, allowedOperations, deniedOperations));
                    }
                }
            }

            return permissions;
        }

        protected List<string>? ReadOperationCollection(JsonElement parent, string permissionPropertyName)
        {
            List<string>? operations = null;

            if (parent.TryGetProperty(permissionPropertyName, out JsonElement operationsColl))
            {
                operations = new List<string>();
                
                foreach (JsonElement operationElement in operationsColl.EnumerateArray())
                {
                    string? operation = operationElement.GetString();
                    
                    if (operation is not null)
                    {
                        operations.Add(operation);
                    }
                }
            }

            return operations;
        }

        public Task<bool> AuthenticateUserAsync(ClaimsPrincipal principal, object? authDataObject)
        {
            Claim? userIdClaim = principal.Claims
                .Where(t => t.Type == ClaimTypes.NameIdentifier)
                .FirstOrDefault();

            if (userIdClaim != null)
            {
                if (users.ContainsKey(userIdClaim.Value))
                {
                    AuthEntry userAuth = users[userIdClaim.Value];

                    if (authDataObject is null || authDataObject.GetType() != typeof(string))
                    {
                        return Task.FromResult(false);
                    }

                    string password = (string)authDataObject;
                    bool isAuthenticated = _passwordEncoder.Matches(password, userAuth.Password).matches;

                    if (isAuthenticated && typeof(ClaimsIdentity).IsAssignableFrom(principal.Identity?.GetType())) // Augment the Principal
                    {
                        var identity = (ClaimsIdentity)principal.Identity;
                        identity.AddClaim(new Claim(identity.NameClaimType, userAuth.Username));

                        if (userAuth.Roles is not null)
                        {
                            foreach (string role in userAuth.Roles)
                            {
                                identity.AddClaim(new Claim(identity.RoleClaimType, role));
                            }
                        }
                    }

                    return Task.FromResult(isAuthenticated);
                }
            }

            throw new FileAuthDataStoreException($"User does not have the claim {ClaimTypes.NameIdentifier}. This is required by this Auth DataStore. The Authentication plugin should add this claim for the user");
        }

        public Task<bool> AuthorizeUserAsync(ClaimsPrincipal principal, object? authDataObject)
        {
            if (authDataObject is null || !typeof(SOAPAuthData).IsAssignableFrom(authDataObject.GetType()))
            {
                throw new FileAuthDataStoreException("FileAuthDataStore expects a SOAPAuthData object. The AuthorizeAttribute should supply this.");
            }

            SOAPAuthData authZData = (SOAPAuthData)authDataObject;

            if (authZData.PerformSOAPOperationAuthorization)
            {
                //Authorization could be called multiple times i.e. if there are multiple attributes on the controller
                //This flag ensures we only perform SOAP Operation authorization once
                authZData.PerformSOAPOperationAuthorization = false;

                // userIdClaim must be present here, otherwise Authentication would have failed
                Claim userIdClaim = principal.Claims
                    .Where(t => t.Type == ClaimTypes.NameIdentifier)
                    .First();
                
                AuthEntry userAuth = users[userIdClaim.Value];
                bool bAuthorized = false;

                // This is a whitelist so deny everything except the entries in the collection
                if (userAuth.AllowedPermissions is not null)
                {
                    bAuthorized = false;
                    
                    var permissionItem = userAuth.AllowedPermissions
                        .FirstOrDefault(x => x.Service == authZData.TargetServiceName);
                    
                    if (permissionItem is not null)
                    {
                        bAuthorized = true;
                        if (permissionItem.AllowedOperations is not null && 
                            !permissionItem.AllowedOperations.Contains(authZData.RequestedSOAPOperationName))
                        {
                            bAuthorized = false;
                        }

                        if (permissionItem.DeniedOperations is not null && 
                            permissionItem.DeniedOperations.Contains(authZData.RequestedSOAPOperationName))
                        {
                            bAuthorized = false;
                        }
                    }
                }
                else
                {
                    // This is a Blacklist so allow everything except the entries in the collection
                    if (userAuth.DeniedPermissions is not null)
                    {
                        bAuthorized = true;
                        
                        var permissionItem = userAuth.DeniedPermissions
                            .FirstOrDefault(x => x.Service == authZData.TargetServiceName);
                        
                        if (permissionItem is not null)
                        {
                            bAuthorized = false;
                            if (permissionItem.AllowedOperations is not null && 
                                permissionItem.AllowedOperations.Contains(authZData.RequestedSOAPOperationName))
                            {
                                bAuthorized = true;
                            }

                            if (permissionItem.DeniedOperations is not null && 
                                !permissionItem.DeniedOperations.Contains(authZData.RequestedSOAPOperationName))
                            {
                                bAuthorized = true;
                            }
                        }
                    }
                    // Neither Allow or Deny has been specified, so take some default action (Deny access)
                    else
                    {
                        bAuthorized = false;
                    }
                }
                return Task.FromResult(bAuthorized);
            }
            return Task.FromResult(true);
        }
    }
}
