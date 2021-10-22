using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BirthdayBot.Services;

namespace BirthdayBot
{
    class Program
    {
        static async Task Main()
        {
            var builder = new HostBuilder();
            builder.ConfigureAppConfiguration(x =>
            {
                var configuration = new ConfigurationBuilder();
                configuration.SetBasePath(Directory.GetCurrentDirectory());
                configuration.AddYamlFile("_config.yml", false, true);

                x.AddConfiguration(configuration.Build());
            });

            builder.ConfigureLogging(x =>
            {
                x.AddConsole();
                x.SetMinimumLevel(LogLevel.Debug);
            });

            builder.ConfigureDiscordHost((context, config) =>
            {
                config.SocketConfig = new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    AlwaysDownloadUsers = true,
                    MessageCacheSize = 100,
                };

                config.Token = context.Configuration["tokens:discord"];
            });

            builder.UseCommandService((context, config) =>
            {
                config.CaseSensitiveCommands = false;
                config.LogLevel = LogSeverity.Verbose;
                config.DefaultRunMode = RunMode.Async;
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<BirthdayService>();
                services.AddHostedService<CommandHandler>();
                services.AddHostedService<BotStatusService>();
            });

            builder.UseConsoleLifetime();

            var host = builder.Build();

            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}
