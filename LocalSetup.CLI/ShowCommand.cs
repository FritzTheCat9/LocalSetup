using Spectre.Console.Cli;

namespace LocalSetup.CLI;

public class ShowCommand : AsyncCommand<ShowCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--use-settings")]
        public bool UseSettings { get; set; }

        [CommandOption("--sln")]
        public string? SolutionPath { get; set; }
    }

    public override async Task<int> ExecuteAsync(
         CommandContext context,
         Settings settings,
         CancellationToken cancellationToken)
    {
        var apps = await AzureLoginService.DiscoverAllFunctionApps();

        List<AppResourceMapping> mappings;

        if (settings.UseSettings)
        {
            var webApps = await AzureLoginService.GetWebApps(apps);
            var configs = await AzureLoginService.FetchConfigs(webApps);

            mappings = MappingBuilder.FromSettings(configs);
        }
        else
        {
            mappings = MappingBuilder.Build(apps);
        }

        if (!string.IsNullOrEmpty(settings.SolutionPath))
        {
            var projects = SolutionParser.GetProjectNames(settings.SolutionPath);

            var joined = SolutionMapper.JoinWithSolution(projects, mappings);

            TableRenderer.RenderWithSolution(joined);
        }
        else
        {
            TableRenderer.Render(mappings);
        }

        return 0;
    }
}
