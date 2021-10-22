using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Models
{
    public class AlertBirthday : GuildCommunicationBase
    {
        public AlertBirthday(User user, BDayGuild guild)
        {
            User = user;
            ChannelID = guild.ChannelID;
            GuildID = guild.GuildID;
        }

        public User User { get; set; }
        
    }
}
