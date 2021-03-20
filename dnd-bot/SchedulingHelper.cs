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
        public SchedulingHelper(DiscordSocketClient client)
        {
            _client = client;
            _client.ReactionAdded += onReactionAdded;
            Setup();
        }

        public void Setup()
        {
            cal = CultureInfo.InvariantCulture.Calendar;
            isScheduling = false;
            _rsvp = new RSVPList
            {
                inPerson = new List<IUser>(),
                online = new List<IUser>(),
                cannotCome = new List<IUser>()
            };
        }

        public async void scheduleSession(string date, SocketCommandContext context)
        {
            isScheduling = true;
            var nums = date.Split('/');
            int year = int.Parse(nums[2]);
            int day = int.Parse(nums[1]);
            int month = int.Parse(nums[0]);
            _date = new DateTime(year, month, day);
            _msg = await context.Channel.SendMessageAsync(null, false, buildEmbed());
        }

        public async Task onReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            updateRSVP(reaction, channel);
        }

        public void updateRSVP(SocketReaction reaction, ISocketMessageChannel channel)
        {
            if(isScheduling && channel.GetMessageAsync(reaction.MessageId) == _msg as IMessage)
            {
                var user = _client.GetUser(reaction.UserId);
                switch (reaction.Emote.Name)
                {
                    //in person
                    case "👍":
                        if(!_rsvp.cannotCome.Contains(user) && !_rsvp.online.Contains(user))
                        {
                            _rsvp.inPerson.Add(user);
                        }
                        break;
                    //virtual
                    case "👌":
                        if (!_rsvp.inPerson.Contains(user) && !_rsvp.cannotCome.Contains(user))
                        {
                            _rsvp.online.Add(user);
                        }
                        break;
                    //can't
                    case "👎":
                        if(!_rsvp.inPerson.Contains(user) && !_rsvp.online.Contains(user))
                        {
                            _rsvp.cannotCome.Add(user);
                        }
                        break;
                    default:
                        break;
                }
                _msg.ModifyAsync(x =>
                {
                    x.Embed = buildEmbed();
                });
            }
        }

        public Embed buildEmbed()
        {
            var ebMsg = new EmbedBuilder();
            ebMsg.WithTitle($"New Session Planned For: {cal.GetDayOfWeek(_date)}, {_date.Month}/{_date.Day}/{_date.Year}");
            ebMsg.WithColor(Color.Blue);
            ebMsg.AddField("In Person attendees:", RSVPList.ToString(_rsvp.inPerson));
            ebMsg.AddField("Virtual attendees:", RSVPList.ToString(_rsvp.online));
            ebMsg.AddField("Can't come:", RSVPList.ToString(_rsvp.cannotCome));
            ebMsg.AddField("Options", "Please add a reaction to this message accordingly. React with :thumbsup: if you can make it in person, :ok_hand: if you can make it virtually , or :thumbsdown: if you can't make it.");
            ebMsg.AddField("Cancel", "To end the scheduling session, use the /endsession command.");
            return ebMsg.Build();
        }



        struct RSVPList
        {
            public List<IUser> inPerson;
            public List<IUser> online;
            public List<IUser> cannotCome;

            public static string ToString(List<IUser> lst)
            {
                StringBuilder strB = new StringBuilder();
                foreach(var user in lst)
                {
                    strB.Append(user.Username + ", ");
                }
                strB.Append(",");
                return strB.ToString().Replace(", ,", "");
            }
        }
    }
}
