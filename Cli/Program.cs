﻿using Spectre.Console.Cli;
using Cli.Shell;
using Cli.Helpers;
using Microsoft.Extensions.Configuration;

// Application entry point

class Program
{
    static int Main(string[] args)
    {
        var app = new CommandApp();
        var commandList = CommandConfig.Commands.ToList();

        // Register commands
        app.Configure(config =>
        {
            commandList.ForEach(
                command =>
                {
                    var commandName = command.Name;
                    var commandType = command.CommandType;

                    try
                    {
                        var addCommandMethod = typeof(IConfigurator)
                        .GetMethod("AddCommand")
                        .MakeGenericMethod(commandType);

                        addCommandMethod.Invoke(config, [commandName]);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Failed to register command '{commandName}': {ex.Message}", ex);
                    }
                }
            );
        });

        SetupConfig();

        // If arguments are provided, run the command directly
        if (args.Length > 0)
        {
            return app.Run(args);
        }

        // Otherwise, start the interactive shell
        return Shell.RunShell(app);
    }

    public static void SetupConfig() {
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }
}
