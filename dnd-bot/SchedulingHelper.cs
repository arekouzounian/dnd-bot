using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace dnd_bot
{
    public class SchedulingHelper
    {
        private DiscordSocketClient _client;
        Calendar cal;
        private RestUserMessage _msg;
        public bool isScheduling;
        private RSVPList _rsvp;
        private DateTime _date;
        private string _time;
        public SchedulingHelper(DiscordSocketClient client)
        {
            _client = client;
            Setup();
            _client.ReactionAdded += onReactionAdded;
        }

        public void Setup()
        {
            cal = CultureInfo.InvariantCulture.Calendar;
            isScheduling = false;
            _rsvp = new RSVPList
            {
                inPerson = new List<IUser>(),
                online = new List<IUser>(),
                cannotCome = new List<IUser>(),
                maybe = new List<IUser>(),
                hasResponded = new List<IUser>()
            };
        }
        //no input validation. too bad.
        public async void scheduleSession(string date, string time, SocketCommandContext context)
        {
            isScheduling = true;
            var nums = date.Split('/');
            var timeNums = time.Split(':');
            int year = int.Parse(nums[2]);
            int day = int.Parse(nums[1]);
            int month = int.Parse(nums[0]);
            _time = time;
            _date = new DateTime(year, month, day);
            _msg = await context.Channel.SendMessageAsync(null, false, buildEmbed());
        }

        public async Task onReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            updateRSVP(reaction, channel);
        }

        public async void updateRSVP(SocketReaction reaction, ISocketMessageChannel channel)
        {
            if (isScheduling && reaction.MessageId == _msg.Id)
            {
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
                await _msg.ModifyAsync(x =>
                {
                    x.Embed = buildEmbed();
                });
            }
        }

        public Embed buildEmbed()
        {
            var ebMsg = new EmbedBuilder();
            ebMsg.WithTitle($"New Session Planned For: {cal.GetDayOfWeek(_date)}, {_date.Month}/{_date.Day}/{_date.Year} at {_time} PST (Military Time)");
            ebMsg.WithColor(Color.Blue);
            ebMsg.AddField("In Person attendees:", RSVPList.ToString(_rsvp.inPerson));
            ebMsg.AddField("Virtual attendees:", RSVPList.ToString(_rsvp.online));
            ebMsg.AddField("Maybe:", RSVPList.ToString(_rsvp.maybe));
            ebMsg.AddField("Can't come:", RSVPList.ToString(_rsvp.cannotCome));
            ebMsg.AddField("Options", "Please add a reaction to this message accordingly. React with :thumbsup: if you can make it in person, :ok_hand: if you can make it virtually, :thumbsdown: if you can't make it, or any other emoji if you might be able to make it, but you're unsure.");
            ebMsg.AddField("Cancel", "To end the scheduling session, use the /endsession command.");
            return ebMsg.Build();
        }



        struct RSVPList
        {
            public List<IUser> inPerson;
            public List<IUser> online;
            public List<IUser> cannotCome;
            public List<IUser> maybe;

            public List<IUser> hasResponded;

            public static string ToString(List<IUser> lst)
            {
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
