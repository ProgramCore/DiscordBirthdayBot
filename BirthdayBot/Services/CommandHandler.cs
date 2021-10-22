using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BirthdayBot.Models;

namespace BirthdayBot.Services
{
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider provider;
        private readonly CommandService service;
        private readonly IConfiguration config;

        public CommandHandler(IServiceProvider _provider, DiscordSocketClient _client, CommandService _service, IConfiguration _config, ILogger<DiscordClientService> _logger) : base(_client, _logger)
        {
            provider = _provider;
            service = _service;
            config = _config;
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message))
            { return; }

            if (message.Source != MessageSource.User)
            { return; }

            var argPos = 0;
            if (!message.HasStringPrefix(config["prefix"], ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos))
            { return; }

            var context = new SocketCommandContext(Client, message);
            await service.ExecuteAsync(context, argPos, provider);
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess)
            {
                await context.Channel.SendMessageAsync($"Error: {result}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Client.MessageReceived += OnMessageReceived;
            service.CommandExecuted += OnCommandExecuted;

            bool allowGreet = bool.Parse(config["greet"]);

            if (allowGreet)
            {
                Client.UserJoined += Client_UserJoined;
            }

            await service.AddModulesAsync(Assembly.GetEntryAssembly(), provider);
        }

        private async Task Client_UserJoined(SocketGuildUser arg)
        {
            await arg.Guild.DefaultChannel.SendMessageAsync($"Looks like we got someone new here.. Be sure to add your birthday (with {config["prefix"]}addme <mm/dd>) so we can celebrate together!");
        }
    }
}
