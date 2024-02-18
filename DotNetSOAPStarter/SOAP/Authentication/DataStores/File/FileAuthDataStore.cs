using DotNetSOAPStarter.SOAP.Authentication.DataStores.File.Model;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;

namespace DotNetSOAPStarter.SOAP.Authentication.DataStores.File
{
    public class FileAuthDataStore : IAuthenticationRepository
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
                    throw new FileNotFoundException("FileAuthDataStore cannot find the Auth Information File Directory");
                string[] files = Directory.GetFiles(baseDir, DefaultAuthDataStoreFileName, SearchOption.TopDirectoryOnly);
                if (files.Count() != 1)
                {
                    files = Directory.GetFiles(baseDir, DefaultAuthDataStoreFileName, SearchOption.AllDirectories);
                    if (files.Count() == 0)
                        throw new FileNotFoundException("FileAuthDataStore cannot find the Auth Information File");
                    if (files.Count() > 1)
                        throw new FileNotFoundException("FileAuthDataStore found more than one Auth Information File, there can be only one!");
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

                    List<String>? roles = null;
                    JsonElement rolesColl;
                    if (userElement.TryGetProperty("Roles", out rolesColl))
                    {
                        roles = new List<string>();
                        foreach (JsonElement roleElement in rolesColl.EnumerateArray())
                        {
                            string? role = roleElement.GetString();
                            if (role is not null)
                                roles.Add(role);
                        }
                    }

                    AuthEntry user = new AuthEntry(userId, username, password, roles);
                    users.Add(new KeyValuePair<string, AuthEntry>(user.UserId, user));
                }
            }

            if (users.Count == 0)
            {
                _logger.LogWarning("No users added to Auth DataStore. This might break the Authentication mechanism.");
            }
        }

        public Task<bool> AuthenticateUserAsync(ClaimsPrincipal principal, Object? authDataObject)
        {
            Claim? userIdClaim = principal.Claims.Where(t => t.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
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
    }
}
