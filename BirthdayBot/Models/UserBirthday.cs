using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Models
{
    public class User
    {
        private User()
        { }

        public User(SocketGuildUser user)
        {
            if(user != null)
            {
                Username = user.Username;
                ID = user.Id;
            }
        }

        public string Username { get; set; }
        public ulong ID { get; set; }
        public DateTime Birthday { get; set; }

        public bool TryParseBirthday(string strDate)
        {
            if (string.IsNullOrWhiteSpace(strDate))
            {
                return false;
            }


            DateTime conv;
            var culture = CultureInfo.GetCultureInfo("en");

            if (DateTime.TryParse(strDate, culture, DateTimeStyles.None, out conv))
            {
                Birthday = conv;
                return true;
            }

            return false;
        }

        public string GetMention()
        {
            return $"<@!{ID}>";
        }
    }
}
