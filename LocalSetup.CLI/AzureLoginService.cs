using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.Resources;
using Azure.Security.KeyVault.Secrets;

namespace LocalSetup.CLI;

public static class AzureLoginService
{
    public static ArmClient GetClient()
    {
        var credential = new DefaultAzureCredential();

        return new ArmClient(credential);
    }

    public static async Task<List<WebSiteResource>> DiscoverFunctionApps(
        ResourceGroupResource rg)
    {
        var list = new List<WebSiteResource>();

        await foreach (var site in rg.GetWebSites().GetAllAsync())
        {
            if (site.Data.Kind?.Contains("functionapp") == true)
            {
                list.Add(site);
            }
        }

        return list;
    }

    public static async Task<List<FunctionAppConfig>> FetchConfigs(List<WebSiteResource> apps)
    {
        var tasks = apps.Select(async app =>
        {
            var response = await app.GetApplicationSettingsAsync();

            var settings = response.Value.Properties
                .ToDictionary(x => x.Key, x => x.Value);

            return new FunctionAppConfig
            {
                Name = app.Data.Name,
                Settings = settings
            };
        });

        return (await Task.WhenAll(tasks)).ToList();
    }

    public static async Task<string> Resolve(string value)
    {
        if (!value.StartsWith("@Microsoft.KeyVault"))
            return value;

        var uri = ExtractSecretUri(value);

        var client = new SecretClient(
            new Uri(uri.GetLeftPart(UriPartial.Authority)),
            new DefaultAzureCredential());

        var secretName = uri.Segments.Last();

        var secret = await client.GetSecretAsync(secretName);

        return secret.Value.Value;
    }

    private static Uri ExtractSecretUri(string keyVaultReference)
    {
        var start = keyVaultReference.IndexOf("SecretUri=") + "SecretUri=".Length;
        var end = keyVaultReference.IndexOf(")", start);

        var uri = keyVaultReference.Substring(start, end - start);

        return new Uri(uri);
    }
}
