using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Models
{
    public class GuildCommunicationBase
    {
        public ulong ChannelID { get; set; }
        public ulong GuildID { get; set; }
    }
}
