using System.Text.RegularExpressions;

namespace LocalSetup.CLI;

public static class EnvironmentParser
{
    public static (string env, string app) Resolve(
        string rg,
        IDictionary<string, string>? settings = null)
    {
        // 1. AppSettings (best)
        if (settings != null &&
            settings.TryGetValue("ASPNETCORE_ENVIRONMENT", out var env) &&
            settings.TryGetValue("APP_NAME", out var app))
        {
            return (env.ToLower(), app);
        }

        // 2. Regex fallback
        var match = Regex.Match(rg, @"\b(dev|stage|prod)\b", RegexOptions.IgnoreCase);

        if (match.Success)
        {
            var foundEnv = match.Value.ToLower();
            var appName = rg.Replace(foundEnv, "").Trim('-');

            return (foundEnv, appName);
        }

        // 3. Last fallback
        return ("unknown", rg);
    }
}
