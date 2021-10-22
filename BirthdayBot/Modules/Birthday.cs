using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BirthdayBot.Services;
using BirthdayBot.Models;
using Discord;
using System.Timers;

namespace BirthdayBot.Modules
{
    /*
     * IDEAS
     * sing a song
     * save in DB
     */

    public class Birthday : Entertainment
    {
        private readonly IConfiguration config;
        private BirthdayService bdayService;
        
        public Birthday(IServiceProvider _provider, BirthdayService bService, IConfiguration configuration) : base(_provider)
        {
            config = configuration;
            bdayService = bService;
        }

        [Command("add")]
        public async Task Add(SocketGuildUser user = null, string datestr = null)
        {
            if(user == null || string.IsNullOrWhiteSpace(datestr))
            {
                var prefix = config["prefix"];
                await Context.Channel.SendMessageAsync($"🚫   To add a birthday, execute [{prefix}add <@mention> <birthday>] where <@mention> is the user and <birthday> the birthday formatted as mm/dd. \nFor Example: {prefix}add {Context.Client.CurrentUser.Mention} 9/21");
                return;
            }

#if !DEBUG
            if (await UserBDayIsRegistered(user))
            {
                await Context.Channel.SendMessageAsync($"{user.Username} is already registered");
                return;
            }
#endif
            var newUser = new User(user);
            
            if(!newUser.TryParseBirthday(datestr))
            {
                await Context.Channel.SendMessageAsync($"🚫   The birthday was not in the mm/dd format and could not be saved");
                return;
            }

            if (newUser.ID == Context.Client.CurrentUser.Id)
            {
                if(newUser.Birthday.DayOfYear != 263) //DayOfYear for bot
                {
                    await Context.Channel.SendMessageAsync($"That's not my birthday!");
                    return;
                }

                await Context.Channel.SendMessageAsync($"Thank you for remembering me!  ❤️");
            }

            await AddBDay(newUser, user.Guild.Id);
        }

        [Command("addme")]
        public async Task AddMe(string datestr = null)
        {
            if (string.IsNullOrWhiteSpace(datestr))
            {
                var prefix = config["prefix"];
                await Context.Channel.SendMessageAsync($"🚫   To add a birthday, execute [{prefix}add <@mention> <mm/dd>]. \nFor Example: {prefix}add {Context.Client.CurrentUser.Mention} 9/21");
                return;
            }

            var user = Context.User as SocketGuildUser;

#if !DEBUG

            if (await UserBDayIsRegistered(user))
            {
                await Context.Channel.SendMessageAsync($"{user.Username} is already registered");
                return;
            }
#endif

            var userbday = new User(user);

            if (!userbday.TryParseBirthday(datestr))
            {
                await Context.Channel.SendMessageAsync($"🚫   The birthday was not in the mm/dd format and could not be saved");
                return;
            }

            await AddBDay(userbday, user.Guild.Id);
        }

        private async Task AddBDay(User user, ulong guildID)
        {
            await bdayService.AddBirthday(user, guildID);
            await Context.Channel.SendMessageAsync($"🎂   Added {user.Username}'s birthday");
        }

        [Command("deleteuser")]
        [Alias("deluser")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DeleteBirthday(SocketGuildUser user = null)
        {
            if(user == null)
            {
                user = Context.User as SocketGuildUser;
            }

            bool isSuccess = await bdayService.DeleteBirthday(user.Id, Context.Guild.Id);
            
            if(isSuccess)
            {
                await Context.Channel.SendMessageAsync($"🗑   Trashed {user.Username}'s birthday");
            }
            else
            {
                await Context.Channel.SendMessageAsync($"🚫   Hmm, couldn't find and delete that birthday.");
            }
        }

        [Command("deleteall")]
        [Alias("delall")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DeleteAll()
        {
            await bdayService.DeleteAll(Context.Guild.Id);
            await Context.Channel.SendMessageAsync($"🗑   Trashed all birthdays");
        }

        [Command("delete")]
        [Alias("del")]
        public async Task DeleteBirthdayMe()
        {
            var user = Context.User as SocketGuildUser;

            await DeleteBirthday(user);
        }

        [Command("upcoming")]
        [Alias("up")]
        public async Task UpcomingBDay(int months = 1)
        {
            if (months < 1)
            {
                months = 1;
            }
            else if (months > 12)
            {
                months = 12;
            }

            var guild = bdayService.GetGuild(Context.Guild.Id);

            if (guild == null)
            { return; }

            if (guild.RegisteredUsers.Count() == 0)
            {
                var prefix = config["prefix"];
                await Context.Channel.SendMessageAsync($"No birthdays added yet! To add, execute [{prefix}add <@mention> <mm/dd>]");
                return;
            }

            var upList = new List<UpcomingBDay>();
            var maxDay = DateTime.Today.DayOfYear + (months * 31);

            foreach(var user in guild.RegisteredUsers)
            {
                var up = new UpcomingBDay(user);

                if(up.ModifiedDayOfYear <= maxDay)
                {
                    upList.Add(up);
                }
            }

            upList.Sort((u1, u2) =>  u1.ModifiedDayOfYear.CompareTo(u2.ModifiedDayOfYear) );

            EmbedBuilder embed = null;

            for (int i = 0; i < upList.Count(); i++)
            {
                if (i == 0 || i % 25 == 0)
                {
                    if (embed?.Fields.Count > 0)
                    {
                        await Context.Channel.SendMessageAsync(null, false, embed.Build());
                    }

                    embed = new EmbedBuilder();

                    if (i == 0)
                    {
                        embed.WithTitle("Upcoming Birthdays   🎂");

                        if (months == 1)
                        {
                            embed.WithDescription("within the next month");
                        }
                        else
                        {
                            embed.WithDescription($"within the next {months} months");
                        }
                    }
                }

                var user = upList[i];
                embed.AddField(user.User.Username, user.User.Birthday.ToString("MM/dd"), true);
            }

            if (embed?.Fields.Count > 0)
            {
                await Context.Channel.SendMessageAsync(null, false, embed.Build());
            }
            else
            {
                var plural = months == 1 ? string.Empty : "s";
                await Context.Channel.SendMessageAsync($"No birthdays between now and {months} month{plural} from now");
            }
        }

        [Command("list")]
        public async Task ListBDays()
        {
            var guild = bdayService.GetGuild(Context.Guild.Id);

            if (guild == null)
            { return; }

            if (guild.RegisteredUsers.Count() == 0)
            {
                var prefix = config["prefix"];
                await Context.Channel.SendMessageAsync($"No birthdays added yet! To add, execute [{prefix}add <@mention> <mm/dd>]");
                return;
            }

            EmbedBuilder embed = null;


            for (int i = 0; i < guild.RegisteredUsers.Count(); i++)
            {
                if (i == 0 || i % 25 == 0)
                {
                    if (embed?.Fields.Count > 0)
                    {
                        await Context.Channel.SendMessageAsync(null, false, embed.Build());
                    }

                    embed = new EmbedBuilder();
                }

                var user = guild.RegisteredUsers[i];
                embed.AddField(user.Username, user.Birthday.ToString("MM/dd"), true);
            }

            if (embed.Fields.Count > 0)
            {
                await Context.Channel.SendMessageAsync(null, false, embed.Build());
            }
        }

        [Command("check")]
        public async Task Check(SocketGuildUser user = null)
        {
            if (user == null)
            {
                user = Context.User as SocketGuildUser;
            }

            if(await UserBDayIsRegistered(user))
            {
                await Context.Channel.SendMessageAsync($"👍   I got you, {user.Username}", false, null);
            }
            else
            {
                var prefix = config["prefix"];
                await Context.Channel.SendMessageAsync($"👎   I don't have {user.Username} added. To add, execute [{prefix}add <@mention> <mm/dd>]", false, null);
            }
        }

        private async Task<bool> UserBDayIsRegistered(SocketGuildUser user)
        {
            return await bdayService.ContainsUserBirthday(user.Id, user.Guild.Id);
        }

        [Command("set")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetChannel()
        {
            await bdayService.SetDefaultChannel(Context.Guild.Id, Context.Channel.Id);
            await Context.Channel.SendMessageAsync("👍   Channel set");
        }
    }
}
