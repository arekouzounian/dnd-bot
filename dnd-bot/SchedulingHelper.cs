﻿using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace dnd_bot
{
    public class SchedulingHelper
    {
        private DiscordSocketClient _client;
        Calendar cal;
        private Dictionary<ulong, RSVPList> schedules; //maps embed msg id to rsvplist

        public SchedulingHelper(DiscordSocketClient client)
        {
            _client = client;
            schedules = new Dictionary<ulong, RSVPList>();
            Setup();
            _client.ReactionAdded += onReactionAdded;
        }

        public void Setup()
        {
            cal = CultureInfo.InvariantCulture.Calendar;
        }

        public async void scheduleSession(string date, string time, SocketCommandContext context)
        {
            foreach(var val in schedules.Values)
            { 
                if(val.initiator.Id == context.User.Id)
                {
                    await context.Channel.SendMessageAsync("You can only schedule one session at a time!");
                    return;
                }
            }


            RSVPList newList = new RSVPList();
            newList.hasResponded = new List<IUser>();
            newList.cannotCome = new List<IUser>();
            newList.online = new List<IUser>();
            newList.inPerson = new List<IUser>();
            newList.maybe = new List<IUser>();
            newList.initiator = context.User;
            var nums = date.Split('/');
            int year = int.Parse(nums[2]);
            int day = int.Parse(nums[1]);
            int month = int.Parse(nums[0]);
            newList.time = time;
            newList.date = new DateTime(year, month, day);
            newList.msg = await context.Channel.SendMessageAsync(null, false, buildEmbed(newList));
            schedules.Add(newList.msg.Id, newList);
        }

        public bool endSession(ulong userId)
        {
            return schedules.Remove(userId);
        }

        public async Task onReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            updateRSVP(reaction, channel);
        }

        public async void updateRSVP(SocketReaction reaction, ISocketMessageChannel channel)
        {
            RSVPList _rsvp;
            if (schedules.ContainsKey(reaction.MessageId))
                _rsvp = schedules[reaction.MessageId];
            else
                return;
            
            var user = _client.GetUser(reaction.UserId);
            if (!_rsvp.hasResponded.Contains(user))
            {
                switch (reaction.Emote.Name)
                {
                    //in person
                    case "👍":
                        _rsvp.inPerson.Add(user);
                        break;
                    //virtual
                    case "👌":
                        _rsvp.online.Add(user);
                        break;
                    //can't
                    case "👎":
                        _rsvp.cannotCome.Add(user);
                        break;
                    default:
                        _rsvp.maybe.Add(user);
                        break;
                }
                _rsvp.hasResponded.Add(user);
            }
            await _rsvp.msg.ModifyAsync(x =>
            {
                x.Embed = buildEmbed(_rsvp);
            });

        }

        public Embed buildEmbed(RSVPList _rsvp)
        {
            var ebMsg = new EmbedBuilder();
            ebMsg.WithTitle($"New Session Planned For: {cal.GetDayOfWeek(_rsvp.date)}, {_rsvp.date.Month}/{_rsvp.date.Day}/{_rsvp.date.Year} at {_rsvp.time} PST (Military Time)");
            ebMsg.WithColor(Color.Blue);
            ebMsg.AddField("In Person attendees:", RSVPList.ToString(_rsvp.inPerson));
            ebMsg.AddField("Virtual attendees:", RSVPList.ToString(_rsvp.online));
            ebMsg.AddField("Maybe:", RSVPList.ToString(_rsvp.maybe));
            ebMsg.AddField("Can't come:", RSVPList.ToString(_rsvp.cannotCome));
            ebMsg.AddField("Options", "Please add a reaction to this message accordingly. React with :thumbsup: if you can make it in person, :ok_hand: if you can make it virtually, :thumbsdown: if you can't make it, or any other emoji if you might be able to make it, but you're unsure.");
            ebMsg.AddField("Cancel", "To end the scheduling session, use the /endschedule command.");
            return ebMsg.Build();
        }



        public struct RSVPList
        {
            public SocketUser initiator;
            public RestUserMessage msg;
            public DateTime date;
            public string time;
            public List<IUser> inPerson;
            public List<IUser> online;
            public List<IUser> cannotCome;
            public List<IUser> maybe;

            public List<IUser> hasResponded;

            public static string ToString(List<IUser> lst)
            {
                if(lst == null)
                {
                    return ",";
                }
                StringBuilder strB = new StringBuilder();
                foreach (var user in lst)
                {
                    strB.Append(user.Username + ", ");
                }
                strB.Append(",");
                return strB.ToString().Replace(", ,", "");
            }

            //public bool hasResponded(IUser user)
            //{
            //    return inPerson.Contains(user) || online.Contains(user) || cannotCome.Contains(user) || maybe.Contains(user);
            //}
        }
    }
}
