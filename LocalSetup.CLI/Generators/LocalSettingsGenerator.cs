using System.Text.Json;

namespace LocalSetup.CLI.Generators;

public class LocalSettingsGenerator
{
    public static void Generate(Dictionary<string, string> settings)
    {
        var obj = new
        {
            IsEncrypted = false,
            Values = settings
        };

        var json = JsonSerializer.Serialize(
            obj,
            new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText("local.settings.json", json);
    }

    public static void GeneratePerFunction(FunctionAppConfig app)
    {
        var json = JsonSerializer.Serialize(
            app.Settings,
            new JsonSerializerOptions { WriteIndented = true });

        Directory.CreateDirectory("env");

        File.WriteAllText($"env/{app.Name}.settings.json", json);
    }
}
