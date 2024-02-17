using System.Text.Json;

namespace DotNetSOAPStarter.SOAP.Authentication.DataStores.File
{
    public static class JsonElementExtensions
    {
        public static string GetMandatoryString(this JsonElement parentElement, string propertyName)
        {
            JsonElement element;
            if (!parentElement.TryGetProperty(propertyName, out element))
                throw new FileAuthDataStoreException($"{propertyName} is mandatory in the Auth Information File");
            return element.ToString();
        }
    }
}
