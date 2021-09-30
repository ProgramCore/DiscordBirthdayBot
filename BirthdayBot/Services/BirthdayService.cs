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
        private const double TIME_INTERVAL = 45000;//3600000;//86400000;
        private IConfiguration config;
        private int checkHour = 12;
        private const int CHECK_MIN = 0;

        public BirthdayService(DiscordSocketClient client, IConfiguration _config)
        {
            config = _config;
            FilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"_birthdays.txt");
            LoadList();
            Client = client;
            Client.Ready += Client_Ready;
            Client.LeftGuild += Client_LeftGuild;
            InitHourOfDayCheck();
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

        private async Task Client_LeftGuild(SocketGuild arg)
        {
            Guilds.RemoveAll(g => g.GuildID == arg.Id);
            await SaveListAsync();
        }

        private async Task Client_Ready()
        {
            await SynchronizedGuilds();

            dayTimer = new System.Timers.Timer(TIME_INTERVAL);
            dayTimer.Elapsed += Timer_Elapsed;

            if(DateTime.Now.Hour >= checkHour) //Time has passed already for the check when the client was initialized
            {
                await TimeTick();
            }

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
            var birthdays = new List<Tuple<User, BDayGuild>>();

            foreach (var guild in Guilds)
            {
                var list = guild.RegisteredUsers.Where(u => u.Birthday.Date.CompareTo(DateTime.Now.Date) == 0).ToList();

                foreach (var user in list)
                {
                    birthdays.Add(new Tuple<User, BDayGuild>(user, guild));
                }
            }

            if (birthdays.Count == 0)
            { return; }

            var rand = new Random();

            foreach (var bday in birthdays)
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

                var channel = Client.GetChannel(bday.Item2.DefaultChannelID) as Discord.IMessageChannel;

                if (channel == null)
                {
                    var guild = Client.GetGuild(bday.Item2.GuildID);
                    channel = Client.GetChannel(guild.DefaultChannel.Id) as Discord.IMessageChannel;
                }

                await channel.SendMessageAsync($"Happy Birthday {bday.Item1.GetMention()}!", false, embed.Build());
                //bday.Item1.Birthday = bday.Item1.Birthday.AddYears(1);
            }

            await SaveListAsync();
        }

        private async Task SynchronizedGuilds()
        {
            var allGuilds = Client.Guilds;

            foreach (var guild in allGuilds)
            {
                if (!Guilds.Any(g => g.GuildID == guild.Id))
                {
                    AddNewGuild(guild.Id, guild.DefaultChannel.Id);
                }
            }

            var removedGuilds = new List<ulong>();

            foreach (var guild in Guilds)
            {
                if (!allGuilds.Any(g => g.Id == guild.GuildID))
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

        public void AddNewGuild(ulong guildID, ulong defaultChannel)
        {
            var guild = new BDayGuild(guildID);
            guild.DefaultChannelID = defaultChannel;

            Guilds.Add(guild);
        }

        public async Task AddBirthday(User bday, ulong guildID)
        {
            if(bday != null)
            {
                InsertBirthdayOrdered(bday, guildID);
                await SaveListAsync();
            }
        }

        private void InsertBirthdayOrdered(User user, ulong guildID)
        {
            /*if(user.Birthday.DayOfYear < DateTime.Now.DayOfYear)
            {
                user.Birthday = user.Birthday.Date.AddYears(1);
            }*/

            var guild = GetGuild(guildID);

            if(guild == null)
            { return; }

            if (guild.RegisteredUsers.Count == 0)
            {
                guild.RegisteredUsers.Add(user);
            }
            else
            {
                var added = false;
                for (int i = 0; i < guild.RegisteredUsers.Count(); i++)
                {
                    var cur = guild.RegisteredUsers[i];

                    if (cur.Birthday.DayOfYear > user.Birthday.DayOfYear)
                    {
                        guild.RegisteredUsers.Insert(i, user);
                        added = true;
                        break;
                    }
                }

                if (!added)
                {
                    guild.RegisteredUsers.Add(user);
                }
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

        private Task SaveList()
        {
            var root = new Root() { Guilds = this.Guilds };
            var json = JsonConvert.SerializeObject(root);
            File.WriteAllText(FilePath, json);
            return Task.CompletedTask;
        }

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
                    /*var now = DateTime.Now;

                    foreach(var guild in Guilds)
                    {
                        foreach (var user in guild.RegisteredUsers)
                        {
                            if (user.Birthday.DayOfYear < now.DayOfYear)
                            {
                                user.Birthday = new DateTime(now.Year + 1, user.Birthday.Month, user.Birthday.Day);
                            }
                            else
                            {
                                user.Birthday = new DateTime(now.Year, user.Birthday.Month, user.Birthday.Day);
                            }
                        }
                    }

                    SaveList();*/
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

            var list = guild.RegisteredUsers.Any(u => u.ID == userid);

            return Task.FromResult(list);
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
