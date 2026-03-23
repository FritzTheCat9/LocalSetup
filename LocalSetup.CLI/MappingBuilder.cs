namespace LocalSetup.CLI;

public static class MappingBuilder
{
    public static List<AppResourceMapping> Build(List<FunctionAppResource> apps)
    {
        var dict = new Dictionary<string, AppResourceMapping>();

        foreach (var app in apps)
        {
            var (env, logicalName) = EnvironmentParser.Resolve(app.ResourceGroup);

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
            var (env, appName) = EnvironmentParser.Resolve(
                app.ResourceGroup,
                app.Settings
            );

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
