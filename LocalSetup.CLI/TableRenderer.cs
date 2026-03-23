namespace LocalSetup.CLI;

using Spectre.Console;

public static class TableRenderer
{
    public static void Render(List<AppResourceMapping> mappings)
    {
        var table = new Table();

        table.AddColumn("App");
        table.AddColumn("Dev");
        table.AddColumn("Test");
        table.AddColumn("Stage");
        table.AddColumn("QA");
        table.AddColumn("Prod");

        foreach (var m in mappings.OrderBy(x => x.AppName))
        {
            table.AddRow(
                m.AppName,
                m.Environments.GetValueOrDefault("dev") ?? "-",
                m.Environments.GetValueOrDefault("test") ?? "-",
                m.Environments.GetValueOrDefault("stage") ?? "-",
                m.Environments.GetValueOrDefault("qa") ?? "-",
                m.Environments.GetValueOrDefault("prod") ?? "-"
            );
        }

        AnsiConsole.Write(table);
    }

    public static void RenderWithSolution(
    List<(string Project, AppResourceMapping? Mapping)> data)
    {
        var table = new Table();

        table.AddColumn("Project (.sln)");
        table.AddColumn("App");
        table.AddColumn("Dev");
        table.AddColumn("Test");
        table.AddColumn("Stage");
        table.AddColumn("QA");
        table.AddColumn("Prod");

        foreach (var row in data)
        {
            table.AddRow(
                row.Project,
                row.Mapping?.AppName ?? "-",
                row.Mapping?.Environments.GetValueOrDefault("dev") ?? "-",
                row.Mapping?.Environments.GetValueOrDefault("test") ?? "-",
                row.Mapping?.Environments.GetValueOrDefault("stage") ?? "-",
                row.Mapping?.Environments.GetValueOrDefault("qa") ?? "-",
                row.Mapping?.Environments.GetValueOrDefault("prod") ?? "-"
            );
        }

        AnsiConsole.Write(table);
    }
}
