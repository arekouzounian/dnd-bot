﻿using Discord.Commands;
using System;
using Discord;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Dangl.Calculator;
using Discord.Rest;
using Discord.Addons.Interactive;
using System.Collections.Generic;

namespace dnd_bot
{
    public class Commands : InteractiveBase<SocketCommandContext>
    {
        public WeaponHelper welper = new WeaponHelper();
        public theStatHandler statHandler = new theStatHandler();
        public SchedulingHelper schelper = new SchedulingHelper();

        [Command("roll")]
        public async Task roll(params string[] args)
        {
            string rollCode = args[0];
            string calculations = "";
            if (args.Length == 1)
            {
                foreach (var character in args[0])
                {
                    if (character > 41 && character < 48 && character != 44 && character != 46)
                    {
                        string[] arr = new string[2];
                        int index = args[0].IndexOf(character);
                        arr[0] = args[0].Substring(0, index);
                        arr[1] = args[0].Substring(index);
                        rollCode = arr[0];
                        calculations = arr[1];
                        break;
                    }
                }
            }
            else
            {
                StringBuilder strB = new StringBuilder();
                foreach (var item in args)
                {
                    item.Replace(" ", "");
                    strB.Append(item);
                }
                await roll(strB.ToString());
                return;
            }
            Random gen = new Random();
            await Context.Channel.SendMessageAsync($"Rolling {rollCode} {calculations}...");
            int num;
            if (int.TryParse(rollCode, out num))
            {
                var sum = Calculator.Calculate($"{num}{calculations}").Result;

                await Context.Channel.SendMessageAsync($"You Rolled {num} {calculations} = {sum}");
            }
            else
            {
                var parsedRollCode = rollCode.Split('d');
                int numberOfRolls = 0;
                int sidedDie = 0;
                if (parsedRollCode.Length == 1)
                {
                    numberOfRolls = 1;
                    sidedDie = int.Parse(parsedRollCode[0]);
                }
                else if (parsedRollCode.Length == 2)
                {
                    numberOfRolls = int.Parse(parsedRollCode[0]);
                    sidedDie = int.Parse(parsedRollCode[1]);
                }
                else
                {
                    await Context.Channel.SendMessageAsync("The command was inputted incorrectly. Try doing: /roll [number of dice to roll]d[number of sides on each dice] [operation] [modifier]");
                    return;
                }

                if (numberOfRolls > 500 || numberOfRolls < 1)
                {
                    await Context.Channel.SendMessageAsync("Incorrect amount of dice. You can only roll up to 500 dice at a time.");
                    return;
                }

                StringBuilder strB = new StringBuilder();
                int sum = 0;
                for (int i = 0; i < numberOfRolls; i++)
                {
                    int roll = gen.Next(1, sidedDie + 1);
                    strB.Append($"{roll}");
                    if (i < numberOfRolls - 1)
                    {
                        strB.Append(" + ");
                    }
                    sum += roll;
                }
                strB.Append(" " + calculations);
                EmbedBuilder eb = new EmbedBuilder();
                eb.WithTitle("Dice Roller");
                eb.WithAuthor(Context.User);
                eb.WithColor(getUserColor(Context.User as SocketGuildUser));
                var charLimit = 1000;
                if (strB.ToString().Length > charLimit)
                {
                    var amtOfMsgs = Math.Ceiling((double)(strB.Length / charLimit));
                    eb.AddField("Notice!", "This is a very large roll that surpasses the character limit, so I have to break it up into parts.");
                    for (int i = 0; i < amtOfMsgs; i++)
                    {
                        var substring = strB.ToString().Substring(i * charLimit, charLimit);
                        eb.AddField($"Rolls Part {i + 1}", substring);
                    }
                }
                else
                {
                    eb.AddField("Rolls: ", strB.ToString());
                }
                eb.AddField("Total: ", Calculator.Calculate($"{sum}{calculations}").Result);

                await Context.Channel.SendMessageAsync(null, false, eb.Build());
            }


        }

        #region miscCommands
        [Command("test")]
        [RequireOwner]
        public async Task test(string testString)
        {

        }

        [Command("help"), Alias("Help")]
        public async Task help()
        {
            var eb = getHelp.helpTextEmbed;
            await Context.Channel.SendMessageAsync(null, false, eb.Build());
        }

        [Command("scheduling")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ScheduleGame(string date)
        {
            //add code to stop it from scheduling multiple games through the execution of multiple commands
            var eb = schelper.scheduleSession(date);
            await Context.Channel.SendMessageAsync(null, false, eb.Build());
        }
        #endregion

        #region APICommands
        [Command("spell")]
        public async Task findSpell(params string[] spellName)
        {
            StringBuilder strB = new StringBuilder();
            foreach (var word in spellName)
            {
                strB.Append($"{word} ");
            }
            SpellHelper spelper = new SpellHelper(strB.ToString());
            spelper.printSpell(Context);

        }

        [Command("monster")]
        public async Task findMonster(params string[] monsterName)
        {
            StringBuilder strB = new StringBuilder();
            foreach (var word in monsterName)
            {
                strB.Append($"{word} ");
            }
            MonsterHelper melper = new MonsterHelper(strB.ToString());
            melper.printMonster(Context);
        }
        #endregion

        #region weaponsCommands
        [Command("addweapon", RunMode = RunMode.Async)]
        public async Task addWeapon()
        {
            var input = GetWeaponInput(Context).Result;
            var weapons = welper.GetWeapons().Weapons;
            bool isValid = true, weaponAdded = false;
            foreach (var weapon in weapons)
            {
                if (weapon.Name.ToLower() == input[0].ToLower())
                {
                    await Context.Channel.SendMessageAsync("Duplicate Weapon Names aren't allowed!");
                    isValid = false;
                }
            }
            //this is hacky, i know. too bad.
            if (isValid)
            {
                weaponAdded = welper.AddWeapon(input[0], input[1], input[2], input[3], Context.User.Id);
            }
            if (weaponAdded)
            {
                await Context.Channel.SendMessageAsync("Weapon Logged Successfully.");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Weapon Log Unsuccessful.");
            }
        }

        [Command("getweapons")]
        public async Task getWeapons()
        {
            var weaponList = welper.GetWeapons();
            if (weaponList.Weapons.Count > 0)
            {
                EmbedBuilder eb = new EmbedBuilder();
                eb.WithTitle($"{Context.User.Username}'s Weapons");
                eb.WithColor(Color.DarkGrey);
                int weaponCount = 0;
                foreach (var weapon in weaponList.Weapons)
                {
                    if (weapon.OwnerID == Context.User.Id)
                    {
                        eb.AddField(weapon.Name, $"Damage Dice: {weapon.DamageDice}" +
                        $"\nDamage Type: {weapon.DamageType}" +
                        $"\nEffects: {weapon.Effects}");
                        weaponCount++;
                    }
                }
                if (weaponCount == 0)
                {
                    eb.AddField("No Weapons Here.", "Please use the 'addweapon' command to add your weapons!");
                }
                await Context.Channel.SendMessageAsync(null, false, eb.Build());
            }
            else
            {
                await Context.Channel.SendMessageAsync("I don't have any weapons stored to display here.");
            }
        }

        [Command("getweapons")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task getWeapons(string userMention)
        {
            var user = getUser(userMention);
            if (user == null)
            {
                return;
            }

            var weaponList = welper.GetWeapons();
            if (weaponList.Weapons.Count > 0)
            {
                EmbedBuilder eb = new EmbedBuilder();
                eb.WithTitle($"{user.Username}'s Weapons");
                eb.WithColor(Color.DarkGrey);
                int weaponCount = 0;
                foreach (var weapon in weaponList.Weapons)
                {
                    if (weapon.OwnerID == user.Id)
                    {
                        eb.AddField(weapon.Name, $"Damage Dice: {weapon.DamageDice}" +
                        $"\nDamage Type: {weapon.DamageType}" +
                        $"\nEffects: {weapon.Effects}");
                        weaponCount++;
                    }
                }
                if (weaponCount == 0)
                {
                    eb.AddField("No Weapons Here.", "Please use the 'addweapon' command to add your weapons!");
                }
                await Context.Channel.SendMessageAsync(null, false, eb.Build());
            }
            else
            {
                await Context.Channel.SendMessageAsync("I don't have any weapons stored to display here.");
            }
        }

        [Command("rolldamage")]
        public async Task rollDamage(params string[] args)
        {
            StringBuilder strB = new StringBuilder();
            foreach (var arg in args)
            {
                strB.Append(arg + " ");
            }
            strB.Append("  ");
            strB.Replace("   ", "");
            var weaponName = strB.ToString();
            bool weaponRolled = false;
            foreach (var weapon in welper.GetWeapons().Weapons)
            {
                if (weapon.Name.ToLower() == weaponName.ToLower() && weapon.OwnerID == Context.User.Id)
                {
                    await roll(weapon.DamageDice);
                    weaponRolled = true;
                }
            }
            if (!weaponRolled)
            {
                await Context.Channel.SendMessageAsync("You don't have that weapon.");
            }
        }

        [Command("removeweapon")]
        public async Task removeWeapon(params string[] args)
        {
            await removeGivenWeapon(formatVariableInput(args), Context.User);
        }
        [Command("removeweapon")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task removeWeapon(string userMention, params string[] args)
        {
            var user = getUser(userMention);
            if (user == null)
            {
                return;
            }

            await removeGivenWeapon(formatVariableInput(args), user);
        }
        private async Task removeGivenWeapon(string weaponName, SocketUser user)
        {
            var wasRemoved = welper.RemoveWeapon(weaponName, user);
            if (wasRemoved)
            {
                await Context.Channel.SendMessageAsync($"Removed {weaponName} successfully.");
            }
            else
            {
                await Context.Channel.SendMessageAsync("I couldn't remove that weapon.");
            }
        }
        #endregion

        #region theStatCommands
        [Command("statAvg"), Alias("statAnvg")]
        public async Task getStatAvg()
        {
            if (statHandler.getCareerAvg(Context.User.Id) == -1)
            {
                await Context.Channel.SendMessageAsync($"I don't have your career average stored yet. Once {Format.Bold("the stat")} has been rolled more, I can compute your average.");
            }
            else
            {
                await Context.Channel.SendMessageAsync($"Your career average for {Format.Bold("the stat")} is: {statHandler.getCareerAvg(Context.User.Id)}.");
            }
        }
        [Command("rollforthestat")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task rollForStat()
        {
            statHandler.rollForTheStat();
        }

        [Command("resetStats")]
        [RequireOwner]
        public async Task resetStatAvg()
        {
            await Context.Channel.SendMessageAsync("Resetting total averages...");
            statHandler.resetStatSheet(statHandler.getStatSheet());
            await Context.Channel.SendMessageAsync("Reset Successful.");
        }
        #endregion

        #region helperFuncs 
        private async Task<string[]> GetWeaponInput(SocketCommandContext context)
        {

            string[] Fields = { "Name", "Damage Dice", "Damage Type", "Misc. Effects" };
            string[] vals = new string[Fields.Length];
            var msg = await context.Channel.SendMessageAsync(null, false, buildWeaponEmbed(Fields, vals, context.User));
            for (int i = 0; i < Fields.Length; i++)
            {
                var askingMsg = await context.Channel.SendMessageAsync($"Please enter the Weapon's {Fields[i]}.");
                var nextMsg = await NextMessageAsync(true, true, new TimeSpan(0, 10, 30));
                vals[i] = nextMsg.Content;

                await askingMsg.DeleteAsync();
                await nextMsg.DeleteAsync();

                await msg.ModifyAsync(x =>
                {
                    x.Embed = buildWeaponEmbed(Fields, vals, context.User);
                });

            }
            await Context.Channel.SendMessageAsync("Done!");


            return vals;
        }
        private string buildOutputString(string[] fields, string[] vals)
        {
            StringBuilder strB = new StringBuilder();
            for (int i = 0; i < fields.Length; i++)
            {
                strB.Append($"{fields[i]}: {vals[i]}\n");
            }

            return strB.ToString();
        }
        private Embed buildWeaponEmbed(string[] fields, string[] vals, SocketUser author)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithTitle("Weapon Builder");
            eb.WithAuthor(author);
            eb.AddField("Specifications", buildOutputString(fields, vals));

            return eb.Build();
        }

        public static async void splitUpLongMessageAsync(string msg, ISocketMessageChannel channel)
        {
            var charLimit = 1000;
            if (msg.Length >= charLimit)
            {
                var amtOfMsgs = Math.Ceiling((double)(msg.Length / charLimit));
                for (int i = 0; i < amtOfMsgs; i++)
                {
                    var substring = msg.Substring(i * charLimit, charLimit);
                    await channel.SendMessageAsync(substring);
                }
            }
            else
            {
                await channel.SendMessageAsync(msg);
            }
        }

        public Color getUserColor(SocketGuildUser user)
        {
            foreach (var role in user.Roles)
            {
                Console.WriteLine(role.Color);
                if (role.Id != 738556835036135467 && role.Id != 738549927537410048 && !user.IsBot)
                {
                    return role.Color;
                }
            }
            return Color.Default;
        }

        private string formatVariableInput(string[] args)
        {
            StringBuilder strB = new StringBuilder();
            foreach (var arg in args)
            {
                strB.Append(arg + " ");
            }

            return strB.ToString().Trim();
        }

        public SocketUser getUser(string userMention)
        {
            SocketUser user = null;
            foreach (var currentUser in Context.Guild.Users)
            {
                if (currentUser.Mention == userMention)
                {
                    user = currentUser;
                }
            }

            return user;
        }
        #endregion
    }
}
