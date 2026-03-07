using Spectre.Console.Cli;
using System.ComponentModel;

namespace LocalSetup.CLI;

public class SyncCommand : AsyncCommand<SyncCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<environment>")]
        public required string Environment { get; set; }

        [CommandOption("--resolve-kv")]
        [DefaultValue(false)]
        public bool ResolveKv { get; set; }
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        var handler = new SyncCommandHandler();

        await handler.Run(settings.Environment, settings.ResolveKv);

        return 0;
    }
}
