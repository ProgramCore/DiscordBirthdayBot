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
        private readonly BirthdayService bdayServ;

        public CommandHandler(IServiceProvider _provider, DiscordSocketClient _client, CommandService _service, IConfiguration _config, ILogger<DiscordClientService> _logger, BirthdayService _bdayService) : base(_client, _logger)
        {
            provider = _provider;
            service = _service;
            config = _config;
            bdayServ = _bdayService;
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
            Client.UserJoined += Client_UserJoined;
            Client.JoinedGuild += Client_JoinedGuild;

            await service.AddModulesAsync(Assembly.GetEntryAssembly(), provider);
        }

        private async Task Client_JoinedGuild(SocketGuild arg)
        {
            bdayServ.AddNewGuild(arg.Id, arg.DefaultChannel.Id);
            await arg.DefaultChannel.SendMessageAsync($"Thanks for inviting me! To get started on adding birthdays, first use the command {config["prefix"]}set on a text channel to set this as the default channel or it will use the first channel on the server. Then type the command {config["prefix"]}add <@mention> <mm/dd> to add a birthday");
        }

        private async Task Client_UserJoined(SocketGuildUser arg)
        {
            await arg.Guild.DefaultChannel.SendMessageAsync($"Looks like we got someone new here.. Be sure to add your birthday (with {config["prefix"]}addme <mm/dd>) so we can celebrate together!");
        }
    }
}
