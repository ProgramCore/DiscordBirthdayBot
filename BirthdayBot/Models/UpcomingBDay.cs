using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Models
{
    public class UpcomingBDay
    {
        private const int MAX_DAY_OF_YEAR = 366;

        public UpcomingBDay(User user)
        {
            User = user;
            SetModifiedDayOfYear();
        }

        private void SetModifiedDayOfYear()
        {
            var today = DateTime.Now.DayOfYear;

            ModifiedDayOfYear = User.Birthday.DayOfYear;

            if (today > ModifiedDayOfYear)
            {
                ModifiedDayOfYear += MAX_DAY_OF_YEAR;
            }

        }

        public User User { get; set; }
        public int ModifiedDayOfYear { get; private set; }
    }
}
