namespace LocalSetup.CLI.Generators;

public class EnvGenerator
{
    public static void Generate(List<FunctionAppConfig> apps)
    {
        var lines = new List<string>();

        foreach (var app in apps)
        {
            foreach (var kv in app.Settings)
            {
                lines.Add($"{kv.Key}={kv.Value}");
            }
        }

        File.WriteAllLines(".env", lines.Distinct());
    }
}
