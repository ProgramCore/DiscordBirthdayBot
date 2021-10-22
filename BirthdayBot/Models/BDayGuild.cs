using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Models
{
    public class BDayGuild : GuildCommunicationBase
    {
        private BDayGuild() { }
        public BDayGuild(ulong guildID, ulong channelID)
        {
            GuildID = guildID;
            ChannelID = channelID;
        }

        public List<User> RegisteredUsers { get; set; } = new List<User>();
    }
}
