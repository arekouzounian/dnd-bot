using Discord;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace dnd_bot
{
    public class SchedulingHelper
    {
        Calendar cal;
        public SchedulingHelper()
        {
            cal = CultureInfo.InvariantCulture.Calendar;
        }

        public EmbedBuilder scheduleSession(string date)
        {
            EmbedBuilder eb = new EmbedBuilder();
            var nums = date.Split('/');
            int year = int.Parse(nums[2]);
            int day = int.Parse(nums[1]);
            int month = int.Parse(nums[0]);
            eb.WithTitle($"New Session Planned For: {cal.GetDayOfWeek(new DateTime(year, month, day))}");
            eb.WithColor(Color.Blue);
            eb.AddField("Options", "Please add a reaction to this message accordingly. React with :desktop: if you can make it virtually, :white_check_mark: if you can make it in person, or :x: if you can't make it.");
            
            return eb;
        }

        //public EmbedBuilder RSVP(EmbedBuilder eb, IUser user)
        //{
        //    var temp = eb;
        //    temp.Fields.FindIndex
        //}
    }
}
