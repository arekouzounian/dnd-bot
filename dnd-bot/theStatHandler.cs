using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace dnd_bot
{
    public class theStatHandler
    {
        private string path;
        public StatSheet stats;
       
        //number of days since stat reset 
        public static int elapseCount;

        public theStatHandler()
        {
            elapseCount = elapseCount == default ? 0 : elapseCount;

            stats = new StatSheet()
            {
                userStats = new Dictionary<ulong, List<double>>(),
            };
            path = Path.Combine(Directory.GetCurrentDirectory(), "stats.json").Replace(@"\", @"\\");
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(stats));
                }
            }
        }


        /// <summary>
        /// Finds the right JSON file along the global path and returns a deserialized object.
        /// </summary>
        /// <returns>A JSON-parsed StatSheet object.</returns>
        public StatSheet getStatSheet()
        {
            using (StreamReader reader = new StreamReader(path))
            {
                return JsonConvert.DeserializeObject<StatSheet>(reader.ReadLine());
            }
        }


        /// <summary>
        /// Serializes the data of the statSheet into JSON to store new information
        /// </summary>
        /// <param name="stats">The given StatSheet to be saved</param>
        private void saveStatSheet(StatSheet stats)
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(JsonConvert.SerializeObject(stats));
            }
        }


        /// <summary>
        /// Resets the given statSheet, removing all previous rolls
        /// </summary>
        /// <param name="stats">The StatSheet that is to be reset.</param>
        public void resetStatSheet(StatSheet stats)
        {
            stats = new StatSheet()
            {
                userStats = new Dictionary<ulong, List<double>>(),
            };
            using(StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(JsonConvert.SerializeObject(stats));
            }
            elapseCount = 0;
        }


        /// <summary>
        /// Adds to Statsheet by rolling random nums and re-calculating averages.
        /// </summary>
        public async void rollForTheStat()
        {
            //initializing variables
            elapseCount++;
            Random gen = new Random();
            var channel = Program.client.GetGuild(738549927537410048).GetChannel(738606355148963950);
            var users = Program.client.GetGuild(738549927537410048).Users;
            await (channel as ISocketMessageChannel).SendMessageAsync($"Time to roll for {Format.Bold("the stat!")}");
            int rollCount = 0; //this stores the total sum of all rolls
            int amtOfRolls = 0; //this stores the amount of all rolls
            var theStatText = Format.Bold("the stat");
            var statSheet = getStatSheet();
            StringBuilder msg = new StringBuilder();
            foreach (var user in users)
            {
                //checking for a eligible user, rolling for them, incrementing requisite variables
                //for final average calculation, and then informing the user of their roll
                if (user.IsBot)
                    continue;
                amtOfRolls++;
                var numRolled = gen.Next(1, 21); 
                rollCount += numRolled;
                msg.Append($"{user.Username}, you rolled {numRolled} for {theStatText} today.\n"); 
                //checking if the statsheet has the user, and if so, adding their roll to their list of rolls
                //if not, add the user to the statSheet, and initialize their list of rolls with their new roll
                if (statSheet.userStats.ContainsKey(user.Id))
                {
                    statSheet.userStats[user.Id].Add(numRolled);
                }
                else
                {
                    statSheet.userStats.Add(user.Id, new List<double> { numRolled });
                }
            }
            saveStatSheet(statSheet);
            msg.Append($"The average roll for {theStatText} today was: {rollCount / amtOfRolls}"); //calculating average
            Commands.splitUpLongMessageAsync(msg.ToString(), channel as ISocketMessageChannel); //sending the final text in as few messages as possible
        }

        /// <summary>
        /// Calculates the global average of a given user's rolls
        /// </summary>
        /// <param name="userId">Numerical ID of user</param>
        /// <returns>A rounded double representing the given user's career stat average</returns>
        public double getCareerAvg(ulong userId)
        {
            var statSheet = getStatSheet();
            if(statSheet.userStats.ContainsKey(userId))
            {
                return GetAvg(statSheet.userStats[userId]);
            }
            else
            {
                return -1; //returns -1 in the case of a user not having a career avg
            }
        }

        public async Task<Tuple<KeyValuePair<IUser, int>, KeyValuePair<IUser, int>, int>> getRecords(SocketCommandContext context)
        {
            KeyValuePair<IUser, int> lowRecord = new KeyValuePair<IUser, int>(null, 0);
            KeyValuePair<IUser, int> highRecord = new KeyValuePair<IUser, int>(null, 0);

            int lowest = 21;
            int highest = 0;

            var users = context.Guild.Users;
            foreach(var user in users)
            {
                if (user.IsBot)
                    continue;
                var avg = (int)getCareerAvg(user.Id);
                if(avg < lowest)
                {
                    lowRecord = new KeyValuePair<IUser, int>(user, avg);
                    lowest = avg;
                }
                if(avg > highest)
                {
                    highRecord = new KeyValuePair<IUser, int>(user, avg);
                    highest = avg;
                }
            }

            return new Tuple<KeyValuePair<IUser, int>, KeyValuePair<IUser, int>, int>(lowRecord, highRecord, elapseCount);
        }


        /// <summary>
        /// Calculates the mean of all rolls contained in the given list
        /// </summary>
        /// <param name="rolls">The list of stat rolls</param>
        /// <returns>The rounded average of the given rolls</returns>
        public double GetAvg(List<double> rolls)
        {
            double sum = 0;
            foreach (var roll in rolls)
            {
                sum += roll;
            }
            return Math.Ceiling(sum / rolls.Count);
        }
    }

    /// <summary>
    /// Class used to store values of rolls for use in average calculations
    /// </summary>
    public class StatSheet
    {
        public Dictionary<ulong, List<double>> userStats; 
    }

    
}
