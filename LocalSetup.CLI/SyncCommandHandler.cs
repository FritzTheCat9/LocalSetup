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
            foreach (var app in configs)
            {
                foreach (var key in app.Settings.Keys.ToList())
                {
                    app.Settings[key] =
                        await AzureLoginService.Resolve(app.Settings[key]);
                }
            }
        }

        EnvGenerator.Generate(configs);

        // merge all settings
        var merged = configs
            .SelectMany(x => x.Settings)
            .ToDictionary(x => x.Key, x => x.Value);

        // generate local.settings.json
        LocalSettingsGenerator.Generate(merged);

        // optional: per-function settings
        foreach (var app in configs)
        {
            LocalSettingsGenerator.GeneratePerFunction(app);
        }
    }
}
