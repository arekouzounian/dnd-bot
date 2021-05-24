using Discord.Commands;
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
        private WeaponHelper _welper = new WeaponHelper();
        private theStatHandler _statHandler = new theStatHandler();
        private SchedulingHelper _schelper = Program.Schelper;
        private SpellHelper _spelper = new SpellHelper();
        private GetHelp _helpHelper = Program.helpComm;
        private bool addingWeap = false;

        #region rollCommand
        [Command("roll"), Summary("Rolls the given damage dice. Usage: /roll [amount of dice]d[amount of sides] + [modifiers]. " +
            "Example: /roll 1d4+1. Note: You don't have to use +, you can use any operator! -, *, /, they all work!")]
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
#endregion

        #region miscCommands
        [Command("test")]
        [RequireOwner]
        public async Task test(string testString)
        {

        }

        [Command("helpme"), Alias("Help", "help")]
        public async Task help()
        {
            var eb = _helpHelper.getHelpEmbed();
            await Context.Channel.SendMessageAsync(null, false, eb.Build());
        }

        [Command("schedule", RunMode = RunMode.Async), Summary("Creates a message that users can react to in order to indicate whether they can make it to the indicated session." +
            "Extra Info: you need to enter a date and a time after the command. Example: /schedule 01/01/2021 21:00. For ease of use, all times are in PST, and are in military time so as not to be confusing.")]
        //[RequireUserPermission(GuildPermission.Administrator)]
        public async Task ScheduleGame(string date, string time)
        {
            _schelper.scheduleSession(date, time, Context);
        }
        [Command("endschedule"), Summary("If there is an existing scheduling tool, this command will end that scheduling tool.")]
        //[RequireUserPermission(GuildPermission.Administrator)]
        public async Task endSchedule()
        {
            var success = _schelper.endSession(Context.User.Id);
            if (!success)
            {
                await Context.Channel.SendMessageAsync("Something went wrong. It doesn't look like you were scheduling a session in the first place.");
            }
        }
        #endregion

        #region APICommands
        [Command("spell"), 
            Summary("This command searches the 5e SRD for the specified spell, and if it finds a match, it will give you a description of that spell! Just enter: /spell [spell name]")]
        public async Task findSpell(params string[] spellName)
        {
            if(spellName[0].ToLower() == "icup")
            {
                await Context.Channel.SendMessageAsync("I-C-U--wait...");
                return;
            }
            if(spellName.Length < 1)
            {
                await Context.Channel.SendMessageAsync("Missing parameter: spell name.");
                return;
            }
            StringBuilder strB = new StringBuilder();
            foreach (var word in spellName)
            {
                strB.Append($"{word} ");
            }
            _spelper.printSpell(Context, strB.ToString());

        }


        //command too buggy. not gonna bother trying to fix it.
        //[Command("monster")]
        //public async Task findMonster(params string[] monsterName)
        //{
        //    await Context.Channel.SendMessageAsync("Sorry! This functionality has been removed.");
        //    StringBuilder strB = new StringBuilder();
        //    foreach (var word in monsterName)
        //    {
        //        strB.Append($"{word} ");
        //    }
        //    MonsterHelper melper = new MonsterHelper(strB.ToString());
        //    melper.printMonster(Context);
        //}
        #endregion

        #region weaponsCommands
        [Command("addweapon", RunMode = RunMode.Async), 
            Summary("Adds a weapon to the user's weapon list! No extra info needed-just enter the command and follow the bot's instructions!")]
        public async Task addWeapon()
        {
            if(addingWeap)
            {
                await Context.Channel.SendMessageAsync("Someone else is adding a weapon right now--try again later!");
                return;
            }
            var input = GetWeaponInput(Context).Result;
            addingWeap = false;

            if (input.Length < 4) //if user timed out or otherwise failed to follow bot instructions
                return;

            var weapons = _welper.GetWeapons().Weapons;
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
                weaponAdded = _welper.AddWeapon(input[0], input[1], input[2], input[3], Context.User.Id);
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

        [Command("getweapons"), Summary("Gets your weapon list. No extra info needed, just enter the command. ")]
        public async Task getWeapons()
        {
            var weaponList = _welper.GetWeapons();
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

        [Command("getweapons"), 
            Summary("Admin command! Get's the specified user's weapons list. The command should look something like this: /getweapons [@user mention]")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task getWeapons(IUser user)
        {
            if (user == null)
            {
                return;
            }

            var weaponList = _welper.GetWeapons();
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

        [Command("rolldamage"), Summary("Rolls the damage for one of your weapons! just enter '/rolldamage [weapon name]' (without the quotes) to execute the command. " +
            "Be careful, though! if the weapon's damage was entered incorrectly (it's not in [number]d[number] + [modifiers] format) it won't work!")]
        public async Task rollDamage(params string[] args)
        {
            var weaponName = formatVariableInput(args);
            bool weaponRolled = false;
            var weapons = _welper.GetWeapons().Weapons;
            foreach (var weapon in weapons)
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

        [Command("removeweapon"), Summary("Removes the given weapon from your weapon list!")]
        public async Task removeWeapon(params string[] args)
        {
            await removeGivenWeapon(formatVariableInput(args), Context.User);
        }
        [Command("removeweapon"), Summary("Admin Command! Removes the weapon from the given user's weapon list. Usage: /removeweapon @user [weapon name]")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task removeWeapon(IUser user, params string[] args)
        {
            await removeGivenWeapon(formatVariableInput(args), user as SocketUser);
        }
        private async Task removeGivenWeapon(string weaponName, SocketUser user)
        {
            var wasRemoved = _welper.RemoveWeapon(weaponName, user);
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
            if (_statHandler.getCareerAvg(Context.User.Id) == -1)
            {
                await Context.Channel.SendMessageAsync($"I don't have your career average stored yet. Once {Format.Bold("the stat")} has been rolled more, I can compute your average.");
            }
            else
            {
                await Context.Channel.SendMessageAsync($"Your career average for {Format.Bold("the stat")} is: {_statHandler.getCareerAvg(Context.User.Id)}.");
            }
        }
        [Command("rollforthestat")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task rollForStat()
        {
            _statHandler.rollForTheStat();
        }

        [Command("resetStats")]
        [RequireOwner]
        public async Task resetStatAvg()
        {
            await Context.Channel.SendMessageAsync("Resetting total averages...");
            _statHandler.resetStatSheet(_statHandler.getStatSheet());
            await Context.Channel.SendMessageAsync("Reset Successful.");
        }
        #endregion

        #region helperFuncs 
        private async Task<string[]> GetWeaponInput(SocketCommandContext context)
        {
            addingWeap = true;
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
