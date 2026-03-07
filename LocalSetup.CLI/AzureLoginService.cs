using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.AppService;
using Azure.Security.KeyVault.Secrets;

namespace LocalSetup.CLI;

public static class AzureLoginService
{
    public static ArmClient GetClient()
        => new ArmClient(new DefaultAzureCredential());

    public static async Task<List<WebSiteResource>> DiscoverFunctionApps(ResourceGroupResource rg)
    {
        var list = new List<WebSiteResource>();

        await foreach (var site in rg.GetWebSites().GetAllAsync())
        {
            if (site.Data.Kind?.Contains("functionapp") == true)
                list.Add(site);
        }

        return list;
    }

    // Fetch all settings in parallel with throttling to avoid rate limits
    public static async Task<List<FunctionAppConfig>> FetchConfigs(List<WebSiteResource> apps)
    {
        var throttler = new SemaphoreSlim(5); // max 5 concurrent requests
        var tasks = apps.Select(async app =>
        {
            await throttler.WaitAsync();
            try
            {
                var response = await app.GetApplicationSettingsAsync();
                return new FunctionAppConfig
                {
                    Name = app.Data.Name,
                    Settings = new Dictionary<string, string>(response.Value.Properties)
                };
            }
            finally
            {
                throttler.Release();
            }
        });

        return (await Task.WhenAll(tasks)).ToList();
    }

    // Resolve KeyVault references
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
        return new Uri(keyVaultReference.Substring(start, end - start));
    }
}