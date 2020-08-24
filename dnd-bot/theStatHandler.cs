using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dnd_bot
{
    public class theStatHandler
    {
        private string path;
        public StatSheet stats;

        public theStatHandler()
        {
            stats = new StatSheet()
            {
                userStats = new Dictionary<ulong, double>(),
                daysPassed = 0
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

        public StatSheet getStatSheet()
        {
            using (StreamReader reader = new StreamReader(path))
            {
                return JsonConvert.DeserializeObject<StatSheet>(reader.ReadLine());
            }
        }

        private void saveStatSheet(StatSheet stats)
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(JsonConvert.SerializeObject(stats));
            }
        }

        public async void rollForTheStat()
        {
            Random gen = new Random();
            var channel = Program.client.GetGuild(738549927537410048).GetChannel(746999483342127104);
            var users = Program.client.GetGuild(738549927537410048).Users;
            await (channel as ISocketMessageChannel).SendMessageAsync($"Time to roll for {Format.Bold("the stat!")}");
            int rollCount = 0;
            int amtOfRolls = 0;
            var theStatText = Format.Bold("the stat");
            var statSheet = getStatSheet();
            foreach (var user in users)
            {
                if (user.IsBot)
                    continue;
                amtOfRolls++;
                var numRolled = gen.Next(1, 21);
                rollCount += numRolled;
                await (channel as ISocketMessageChannel).SendMessageAsync($"{user.Username}, you rolled {numRolled} for {theStatText} today.");
                //add to career avg
                if(statSheet.userStats.ContainsKey(user.Id))
                {
                    double avg;
                    statSheet.userStats.TryGetValue(user.Id, out avg);
                    if(statSheet.daysPassed > 1)
                    {
                        avg = ((avg * (statSheet.daysPassed - 1)) + numRolled) / (statSheet.daysPassed);
                    }
                    else
                    {
                        avg += numRolled;
                        avg /= 2;
                    }
                    statSheet.userStats[user.Id] = avg;
                }
                else
                {
                    statSheet.userStats.Add(user.Id, numRolled);
                }
            }
            statSheet.daysPassed++;
            saveStatSheet(statSheet);
            await (channel as ISocketMessageChannel).SendMessageAsync($"The average roll for {theStatText} today was: {rollCount / amtOfRolls}"); //finding the average
        }

        public int getCareerAvg(ulong userId)
        {
            var statSheet = getStatSheet();
            if(statSheet.userStats.ContainsKey(userId))
            {
                return (int)statSheet.userStats[userId];
            }
            else
            {
                return -1; //returns -1 in the case of a user not having a career avg
            }
        }
    }
    public class StatSheet
    {
        public Dictionary<ulong, double> userStats;
        public int daysPassed;
    }
}
