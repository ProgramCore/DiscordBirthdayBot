using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Models
{
    public class BDayGuild
    {
        private BDayGuild() { }
        public BDayGuild(ulong guildID)
        {
            GuildID = guildID;
        }

        public ulong GuildID { get; }
        public ulong DefaultChannelID { get; set; }

        public List<User> RegisteredUsers { get; set; } = new List<User>();
    }
}
