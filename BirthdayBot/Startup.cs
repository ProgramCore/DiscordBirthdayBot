using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddYamlFile("_config.yml");

            Configuration = builder.Build();
        }

        public static async Task RunAsync(string[] args)
        {
            var startup = new Startup(args);
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
            var services = ConfigureServices();

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<Services.CommandHandler>();

            await provider.GetRequiredService<Services.StartupService>().StartAsync();
            await Task.Delay(-1);
        }

        private ServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = Discord.LogSeverity.Verbose,
                MessageCacheSize = 1000
            }));

            services.AddSingleton(new CommandService(new CommandServiceConfig()
            {
                LogLevel = Discord.LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false
            }));

            services.AddSingleton<Services.CommandHandler>();
            services.AddSingleton<Services.StartupService>();
            services.AddSingleton(Configuration);

            return services;
        }
    }
}
