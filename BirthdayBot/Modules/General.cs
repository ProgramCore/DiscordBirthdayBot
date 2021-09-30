using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace BirthdayBot.Modules
{
    public class General : ModuleBase
    {
        private readonly IConfiguration config;

        public General(IServiceProvider _provider)
        {
            config = _provider.GetService(typeof(IConfiguration)) as IConfiguration;
        }

        [Command("ping")]
        public async Task Ping()
        {
            var client = Context.Client as DiscordSocketClient;

            await Context.Channel.SendMessageAsync($"Pong! ({client.Latency}ms)");
        }

        [Command("bot")]
        public async Task Info()
        {
            var builder = new EmbedBuilder();
            builder.WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl());
            builder.WithTitle($"My name is {Context.Client.CurrentUser.Username}");
            builder.WithDescription($"I'm here to celebrate everyone's birthdays!");
            builder.AddField("My Birthday", "9/20", false);
            builder.AddField("Creator", "Blake Young <programcore.github.io>", false);
            builder.WithFooter("Thank you Discord.Net and GIPHY");

            await Context.Channel.SendMessageAsync(null, false, builder.Build());
        }

        [Command("commands")]
        [Alias("help", "info", "tut", "tutorial")]
        public async Task Commands()
        {
            var prefix = config["prefix"];
            var builder = new EmbedBuilder();
            builder.WithTitle($"{Context.Client.CurrentUser.Username} Commands");
            builder.WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl() ?? Context.Client.CurrentUser.GetDefaultAvatarUrl());
            builder.WithDescription("These are the commands to make me work");
            builder.AddField("Set default channel", $"{prefix}set", false);
            builder.AddField("Add another's birthday", $"{prefix}add <@mention> <birthday mm/dd>", false);
            builder.AddField("Add your birthday", $"{prefix}addme <birthday mm/dd>", false);
            builder.AddField("Delete your birthday", $"{prefix}delete [or {prefix}del]", false);
            builder.AddField("See upcoming birthdays", $"{prefix}upcoming [or {prefix}up]", false);
            builder.AddField("List all birthdays", $"{prefix}list", false);
            builder.AddField("Check if a birthday is registered", $"{prefix}check <@mention>", false);
            builder.AddField("Pick a random thing of a set", $"{prefix}pick <\"option1\"> <\"option2\"> <\"option3\">] or however many you would like", false);
            builder.AddField("See admin commands", $"{prefix}admincommands", false);
            builder.AddField("See info about this bot", $"{prefix}bot", false);

            await Context.Channel.SendMessageAsync(null, false, builder.Build());
        }

        [Command("admincommands")]
        [Alias("adminhelp", "admininfo", "admintut", "admintutorial")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AdminCommands()
        {
            var prefix = config["prefix"];
            var builder = new EmbedBuilder();
            builder.WithTitle($"{Context.Client.CurrentUser.Username} Admin Commands");
            builder.WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl() ?? Context.Client.CurrentUser.GetDefaultAvatarUrl());
            builder.AddField("Set default channel", $"{prefix}set", false);
            builder.AddField("Delete someones birthday", $"{prefix}deleteuser [or {prefix}deluser] <@mention>", false);
            builder.AddField("Delete all birthdays", $"{prefix}deleteall [or {prefix}delall]", false);

            await Context.Channel.SendMessageAsync(null, false, builder.Build());
        }
    }
}
