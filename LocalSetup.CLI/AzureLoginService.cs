using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.AppService;
using Azure.Security.KeyVault.Secrets;

namespace LocalSetup.CLI;

public static class AzureLoginService
{
    public static ArmClient GetClient()
    {
        var credential = new AzureCliCredential();
        return new ArmClient(credential);
    }

    private static readonly Dictionary<string, SecretClient> _clients = [];

    private static SecretClient GetClient(Uri vaultUri)
    {
        if (!_clients.TryGetValue(vaultUri.Host, out var client))
        {
            client = new SecretClient(vaultUri, new AzureCliCredential());
            _clients[vaultUri.Host] = client;
        }

        return client;
    }

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

        var (vaultUri, secretName) = ExtractSecretInfo(value);

        var client = GetClient(vaultUri);

        var secret = await client.GetSecretAsync(secretName);

        return secret.Value.Value;
    }

    private static (Uri vaultUri, string secretName) ExtractSecretInfo(string keyVaultReference)
    {
        var contentStart = keyVaultReference.IndexOf("(") + 1;
        var contentEnd = keyVaultReference.IndexOf(")", contentStart);
        var content = keyVaultReference.Substring(contentStart, contentEnd - contentStart);

        var parts = content.Split(';', StringSplitOptions.RemoveEmptyEntries)
                           .Select(p => p.Split('=', 2))
                           .ToDictionary(p => p[0], p => p[1]);

        // Format 1: SecretUri
        if (parts.ContainsKey("SecretUri"))
        {
            var uri = new Uri(parts["SecretUri"]);
            var vaultUri = new Uri(uri.GetLeftPart(UriPartial.Authority));
            var secretName = uri.Segments.Last();
            return (vaultUri, secretName);
        }

        // Format 2: VaultName + SecretName
        if (parts.ContainsKey("VaultName") && parts.ContainsKey("SecretName"))
        {
            var vaultUri = new Uri($"https://{parts["VaultName"]}.vault.azure.net/");
            var secretName = parts["SecretName"];
            return (vaultUri, secretName);
        }

        throw new InvalidOperationException("Unsupported KeyVault reference format");
    }
}