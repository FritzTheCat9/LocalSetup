using LocalSetup.CLI.Generators;

namespace LocalSetup.CLI;

public class SyncCommandHandler
{
    public async Task Run(string environment, bool resolveKv)
    {
        var client = AzureLoginService.GetClient();
        var subscription = await client.GetDefaultSubscriptionAsync();
        var rg = await subscription.GetResourceGroups().GetAsync(environment);

        var apps = await AzureLoginService.DiscoverFunctionApps(rg.Value);
        var configs = await AzureLoginService.FetchConfigs(apps);

        if (resolveKv)
        {
            // resolve KeyVault secrets in parallel
            var tasks = configs.Select(async app =>
            {
                foreach (var key in app.Settings.Keys.ToList())
                    app.Settings[key] = await AzureLoginService.Resolve(app.Settings[key]);
            });

            await Task.WhenAll(tasks);
        }

        // Generate files
        EnvGenerator.Generate(configs);

        // merge all settings for local.settings.json
        var merged = configs.SelectMany(c => c.Settings)
                            .ToDictionary(kv => kv.Key, kv => kv.Value);

        LocalSettingsGenerator.Generate(merged);

        // optional: per-function files
        foreach (var app in configs)
            LocalSettingsGenerator.GeneratePerFunction(app);
    }
}