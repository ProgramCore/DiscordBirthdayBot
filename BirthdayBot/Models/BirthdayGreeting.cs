using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Models
{
    public static class BirthdayGreeting
    {
        public static string[] Intros = new string[] { 
            "Do I smell cake?", 
            "Another year gone by already?", 
            "This just in!", 
            "Aging like fine wine", 
            "Today is gunna be a good day",
            "I hope you wore your birthday suit today",
            "Party time!",
            "HAPPY BIRTHDAY",
            "It is someone's special day",
            "The day is yours!",
            "Wishes all around",
            "Grab a big slice of cake!",
            "Another year better",
            "Bring the gifts in!"
        };

        public static string GetRandomIntro(Random rand)
        {
            var val = rand.Next(0, Intros.Length);
            return Intros[val];
        }
    }
}
