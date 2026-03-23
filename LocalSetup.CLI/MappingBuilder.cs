namespace LocalSetup.CLI;

public static class MappingBuilder
{
    public static List<AppResourceMapping> Build(List<FunctionAppResource> apps)
    {
        var dict = new Dictionary<string, AppResourceMapping>();

        foreach (var app in apps)
        {
            var (env, logicalName) = EnvironmentParser.Parse(app.ResourceGroup);

            if (!dict.TryGetValue(logicalName, out var mapping))
            {
                mapping = new AppResourceMapping
                {
                    AppName = logicalName
                };
                dict[logicalName] = mapping;
            }

            mapping.Environments[env] = app.ResourceGroup;
        }

        return [.. dict.Values];
    }

    public static List<AppResourceMapping> FromSettings(List<FunctionAppConfig> configs)
    {
        var dict = new Dictionary<string, AppResourceMapping>();

        foreach (var app in configs)
        {
            var settings = app.Settings;

            if (!settings.TryGetValue("ASPNETCORE_ENVIRONMENT", out var env))
                env = "unknown";

            if (!settings.TryGetValue("APP_NAME", out var appName))
                appName = app.Name; // fallback

            if (!dict.TryGetValue(appName, out var mapping))
            {
                mapping = new AppResourceMapping
                {
                    AppName = appName
                };
                dict[appName] = mapping;
            }

            mapping.Environments[env.ToLower()] = app.Name;
        }

        return dict.Values.ToList();
    }
}
