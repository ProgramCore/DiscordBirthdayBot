using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BirthdayBot.Models;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace BirthdayBot.Services
{
    //List saves in order by Day Of Year
    public class BirthdayService
    {
        public List<BDayGuild> Guilds { get; set; } = new List<BDayGuild>();
        public string FilePath { get; set; }
        DiscordSocketClient Client { get; set; }
        private System.Timers.Timer dayTimer;
        private const double TIME_INTERVAL = 60000;//3600000;//86400000;
        private IConfiguration config;
        private int checkHour = 12;
        private const int CHECK_MIN = 0;

        public BirthdayService(DiscordSocketClient client, IConfiguration _config)
        {
            config = _config;
            FilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"_birthdays.txt");
            LoadList();

            InitClient(client);
            InitHourOfDayCheck();
            InitTimer();
        }

        private void InitClient(DiscordSocketClient client)
        {
            Client = client;
            Client.Ready += Client_Ready;
            Client.JoinedGuild += Client_JoinedGuild;
            Client.LeftGuild += Client_LeftGuild;
        }

        private void InitHourOfDayCheck()
        {
            int val = 12;
            if(int.TryParse(config["hour_of_day_to_check_for_birthdays"], out val))
            {
                if(val >= 0 || val <= 23)
                {
                    checkHour = val;
                }
            }
        }

        private async Task Client_JoinedGuild(SocketGuild arg)
        {
            AddNewGuild(arg);
            await SaveListAsync();
            await arg.DefaultChannel.SendMessageAsync($"Thanks for inviting me! To get started on adding birthdays, first use the command {config["prefix"]}set on a text channel to set this as the default channel or it will use the first channel on the server. Then type the command {config["prefix"]}add <@mention> <mm/dd> to add a birthday");
        }

        private async Task Client_LeftGuild(SocketGuild arg)
        {
            Guilds.RemoveAll(g => g.GuildID == arg.Id);
            await SaveListAsync();
        }

        private async Task Client_Ready()
        {
            await SynchronizedGuilds();

            if (DateTime.Now.Hour >= checkHour) //Time has passed already for the check when the client was initialized
            {
                await TimeTick();
            }
        }

        private void InitTimer()
        {
            dayTimer = new System.Timers.Timer(TIME_INTERVAL);
            dayTimer.Elapsed += Timer_Elapsed;
            dayTimer.Enabled = true;
            dayTimer.Start();
        }

        private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(DateTime.Now.Hour == checkHour && DateTime.Now.Minute == CHECK_MIN)
            {
                await TimeTick();
            }
        }

        private async Task TimeTick()
        {
            var birthdaysToday = new List<AlertBirthday>();

            foreach (var guild in Guilds)
            {
                var list = guild.RegisteredUsers.Where(u => u.Birthday.DayOfYear == DateTime.Now.DayOfYear).ToList();

                foreach (var user in list)
                {
                    birthdaysToday.Add(new AlertBirthday(user, guild));
                }
            }

            if (birthdaysToday.Count == 0)
            { return; }

            var rand = new Random();

            foreach (var bday in birthdaysToday)
            {
                var embed = new Discord.EmbedBuilder();
                var urls = await Modules.Entertainment.GetBirthdayUrls(config["tokens:giphy"]);
                
                embed.WithTitle(BirthdayGreeting.GetRandomIntro(rand));
                
                if(!string.IsNullOrWhiteSpace(urls.ImageURL))
                {
                    embed.WithImageUrl(urls.ImageURL);
                    embed.WithFooter("Powered By GIPHY");
                }
                else
                {
                    embed.WithDescription("We wish you all the best on your special day. Do your thing, eat some cake, spoil yourself, and make today all yours. You deserve it. 🍰");
                }

                var channel = Client.GetChannel(bday.ChannelID) as Discord.IMessageChannel;

                if (channel == null)
                {
                    var guild = Client.GetGuild(bday.GuildID);
                    channel = Client.GetChannel(guild.DefaultChannel.Id) as Discord.IMessageChannel;
                    await SetDefaultChannel(bday.GuildID, channel.Id);
                }

                await channel.SendMessageAsync($"Happy Birthday {bday.User.GetMention()}!", false, embed.Build());
            }
        }

        private async Task SynchronizedGuilds()
        {
            foreach (var guild in Client.Guilds)
            {
                if (!Guilds.Any(g => g.GuildID == guild.Id))
                {
                    AddNewGuild(guild);
                }
            }

            var removedGuilds = new List<ulong>();

            foreach (var guild in Guilds)
            {
                if (!Client.Guilds.Any(g => g.Id == guild.GuildID))
                {
                    removedGuilds.Add(guild.GuildID);
                }
            }

            foreach(var id in removedGuilds)
            {
                Guilds.RemoveAll(g => g.GuildID == id);
            }

            await SaveListAsync();
        }

        public void AddNewGuild(SocketGuild sguild)
        {
            Guilds.Add(new BDayGuild(sguild.Id, sguild.DefaultChannel.Id));
        }

        public async Task AddBirthday(User bday, ulong guildID)
        {
            if(bday != null)
            {
                InsertBirthdayOrdered(bday, guildID);
                await SaveListAsync();
            }
        }

        private void InsertBirthdayOrdered(User newUser, ulong guildID)
        {
            var guild = GetGuild(guildID);

            if(guild == null)
            { return; }

            var added = false;
            for (int i = 0; i < guild.RegisteredUsers.Count(); i++)
            {
                var curUser = guild.RegisteredUsers[i];

                if (curUser.Birthday.DayOfYear > newUser.Birthday.DayOfYear)
                {
                    guild.RegisteredUsers.Insert(i, newUser);
                    added = true;
                    break;
                }
            }

            if (!added)
            {
                guild.RegisteredUsers.Add(newUser);
            }
        }

        public async Task<bool> DeleteBirthday(ulong userID, ulong guildID)
        {
            var guild = GetGuild(guildID);

            if(guild == null)
            { return false; }

            var delCount = guild.RegisteredUsers.RemoveAll(u => u.ID == userID);
            
            if(delCount > 0)
            {
                await SaveListAsync();
                return true;
            }

            return false;
        }

        public async Task SaveListAsync()
        {
            var root = new Root() { Guilds = this.Guilds };
            var json = JsonConvert.SerializeObject(root);
            await File.WriteAllTextAsync(FilePath, json);
        }

        /*private Task SaveList()
        {
            var root = new Root() { Guilds = this.Guilds };
            var json = JsonConvert.SerializeObject(root);
            File.WriteAllText(FilePath, json);
            return Task.CompletedTask;
        }*/

        /*public async Task LoadListAsync()
        {
            if(File.Exists(FilePath))
            {
                var jsonStr = await File.ReadAllTextAsync(FilePath);
                var root = JsonConvert.DeserializeObject<Root>(jsonStr);

                if(root != null)
                {
                    Birthdays = root.Birthdays;
                }
            }
        }*/

        private Task LoadList()
        {
            if (File.Exists(FilePath))
            {
                var jsonStr = File.ReadAllText(FilePath);
                var root = JsonConvert.DeserializeObject<Root>(jsonStr);

                if (root != null)
                {
                    Guilds = root.Guilds;
                }
            }

            return Task.CompletedTask;
        }

        public async Task DeleteAll(ulong guildID)
        {
            var guild = GetGuild(guildID);

            if(guild == null)
            { return; }

            guild.RegisteredUsers.Clear();
            await SaveListAsync();
        }

        public Task<bool> ContainsUserBirthday(ulong userid, ulong guildID)
        {
            var guild = GetGuild(guildID);

            if(guild == null)
            { return Task.FromResult(false); }

            var doesContain = guild.RegisteredUsers.Any(u => u.ID == userid);

            return Task.FromResult(doesContain);
        }

        private class Root
        {
            public List<BDayGuild> Guilds { get; set; }
        }

        public BDayGuild GetGuild(ulong id)
        {
            return Guilds.First(g => g.GuildID == id);
        }

        public async Task SetDefaultChannel(ulong guildid, ulong channelid)
        {
            var guild = GetGuild(guildid);
            guild.DefaultChannelID = channelid;
            await SaveListAsync();
        }
    }
}
