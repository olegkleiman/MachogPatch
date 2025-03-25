using Microsoft.Extensions.Configuration;

namespace MachogPatch.Utils
{
    internal class ConfigUtils
    {
        static public void ConfigureHttpClient(IConfigurationSection section, HttpClient httpClient)
        {
            string? url = section["url"];
            httpClient.BaseAddress = new Uri(url);

            string? headerName = section["subscriptionHeader"];
            string? headerValue = section["headerValue"];
            if (headerName is not null
                && headerValue is not null)
                httpClient.DefaultRequestHeaders.Add(headerName, headerValue);
        }
    }
}
