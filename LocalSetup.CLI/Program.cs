using Spectre.Console.Cli;
using LocalSetup.CLI;

var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<SyncCommand>("sync")
        .WithDescription("Sync Azure environment settings");

    config.AddCommand<ShowCommand>("show")
        .WithDescription("Show Function Apps across environments");
});

return app.Run(args);
