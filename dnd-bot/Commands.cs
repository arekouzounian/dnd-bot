using Discord.Commands;
using System;
using Discord;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Dangl.Calculator;
using Discord.Rest;

namespace dnd_bot
{
    public class Commands : ModuleBase<SocketCommandContext>
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
            if(int.TryParse(rollCode, out num))
            {
                var sum = Calculator.Calculate($"{num}{calculations}").Result;

                await Context.Channel.SendMessageAsync($"You Rolled {num} {calculations} = {sum}");
            }
            else
            {
                var parsedRollCode = rollCode.Split('d');
                int numberOfRolls = 0;
                int sidedDie = 0;
                if(parsedRollCode.Length == 1)
                {
                    numberOfRolls = 1;
                    sidedDie = int.Parse(parsedRollCode[0]);
                }
                else if(parsedRollCode.Length == 2)
                {
                    numberOfRolls = int.Parse(parsedRollCode[0]);
                    sidedDie = int.Parse(parsedRollCode[1]);
                }
                else
                {
                    await Context.Channel.SendMessageAsync("The command was inputted incorrectly. Try doing: /roll [number of dice to roll]d[number of sides on each dice] [operation] [modifier]");
                    return;
                }

                if(numberOfRolls > 500 || numberOfRolls < 1)
                {
                    await Context.Channel.SendMessageAsync("Incorrect amount of dice. You can only roll up to 500 dice at a time.");
                    return;
                }

                StringBuilder strB = new StringBuilder();
                int sum = 0; 
                for(int i = 0; i < numberOfRolls; i++)
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
            
            #region shitCode
            /*
            int num;
            if (int.TryParse(rollCode, out num))
            {
                await Context.Channel.SendMessageAsync(Format.Code($"You Rolled {num}."));
                if (num == 17)
                {
                    await Context.Channel.SendMessageAsync(Format.Bold(Format.Italics("That doesn't hit.")));
                }
                return;
            }
            else
            {
                var nums = rollCode.Split('d');
                if (nums.Length != 2)
                {
                    await Context.Channel.SendMessageAsync($"That is an invalid amount of dice. The amount should look like this: [number of dice]d[number of sides on each given die] (+ or -) [modifier]");
                    return;
                }
                if (int.Parse(nums[0]) > 500)
                {
                    await Context.Channel.SendMessageAsync("That is too many dice! You can only roll up to 500 dice at a time.");
                    return;
                }

                int[] rolls = new int[int.Parse(nums[0])];
                for (int i = 0; i < rolls.Length; i++)
                {
                    rolls[i] = gen.Next(1, int.Parse(nums[1]) + 1);
                }
                StringBuilder strB = new StringBuilder();
                int sum = 0;
                for (int i = 0; i < rolls.Length; i++)
                {
                    if (i == rolls.Length - 1 && modifier == 0)
                    {
                        strB.Append(rolls[i]);
                    }
                    else
                    {
                        strB.Append($"{rolls[i]} + ");
                    }
                    sum += rolls[i];
                }
                if (modifier != 0)
                {
                    if (operation == '+')
                    {
                        strB.Append($" {operation} {modifier}");
                        sum += modifier;
                    }
                    else if (operation == '-')
                    {
                        strB.Append($" {operation} {modifier}");
                        sum -= modifier;
                    }
                    else if (operation == '/')
                    {
                        strB.Append($" {operation} {modifier}");
                        sum /= modifier;
                    }
                    else if (operation == '*')
                    {
                        strB.Append($" {operation} {modifier}");
                        sum *= modifier;
                    }
                }
                var charLimit = 1900;

                if (strB.ToString().Length > charLimit)
                {
                    var amtOfMsgs = Math.Ceiling((double)(strB.Length / charLimit));
                    for (int i = 0; i < amtOfMsgs; i++)
                    {
                        var substring = Format.Code(strB.ToString().Substring(i * charLimit, charLimit));
                        await Context.Channel.SendMessageAsync(substring);
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync(Format.Code($"{strB.ToString()} {operation} {modifier}"));
                }

                await Context.Channel.SendMessageAsync($"Total: {sum}");

                if (int.Parse(nums[1]) == 20)
                {
                    if (rolls.Length == 1 && rolls[0] == 20)
                    {
                        await Context.Channel.SendMessageAsync(Format.Bold(Format.Italics("Critical Success!")));
                    }
                    else if (rolls.Length == 1 && rolls[0] == 1)
                    {
                        await Context.Channel.SendMessageAsync(Format.Bold(Format.Italics("Critical Failure...")));
                    }
                    else if (rolls.Length > 1)
                    {
                        int winCount = 0;
                        int failCount = 0;
                        foreach (var roll in rolls)
                        {
                            if (roll == 1)
                            {
                                failCount++;
                            }
                            else if (roll == 20)
                            {
                                winCount++;
                            }
                        }
                        await Context.Channel.SendMessageAsync($"You rolled {winCount} natural 20s and {failCount} natural 1s!");
                    }
                }
            }*/
            #endregion
        }
        public Color getUserColor(SocketGuildUser user)
        {
            foreach(var role in user.Roles)
            {
                Console.WriteLine(role.Color);
                if (role.Id != 738556835036135467 && role.Id != 738549927537410048 && !user.IsBot)
                {
                    return role.Color;
                }
            }
            return Color.Default;
        }

        [Command("test")]
        [RequireOwner]
        public async Task test(string testString)
        {
            
        }

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
        //[RequireUserPermission(GuildPermission.Administrator)]
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

        [Command("help"), Alias("Help")]
        public async Task help()
        {
            var eb = getHelp.helpTextEmbed;
            await Context.Channel.SendMessageAsync(null, false, eb.Build());
        }

        [Command("rollforthestat")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task rollForStat()
        {
            statHandler.rollForTheStat();
        }

        [Command("addweapon")]
        //[RequireOwner]
        public async Task addWeapon(string name, string damage, string damageType, string effects)
        {
            var weaponAdded = welper.AddWeapon(name, damage, damageType, effects, Context.User.Id);
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
        //[RequireOwner]
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
            foreach(var arg in args)
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
        [Command("resetStats")]
        [RequireOwner]
        public async Task resetStatAvg()
        {
            await Context.Channel.SendMessageAsync("Resetting total averages...");
            statHandler.resetStatSheet(statHandler.getStatSheet());
            await Context.Channel.SendMessageAsync("Reset Successful.");
        }

        [Command("removeweapon")]
        public async Task removeWeapon(string weaponName)
        {
            await removeGivenWeapon(weaponName, Context.User);
        }
        [Command("removeweapon")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task removeWeapon(string weaponName, string userMention)
        {
            var user = getUser(userMention);
            if (user == null)
            {
                return;
            }

            await removeGivenWeapon(weaponName, user);
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

        [Command("scheduling")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ScheduleGame(string date)
        {
            //add code to stop it from scheduling multiple games through the execution of multiple commands
            var eb = schelper.scheduleSession(date);
            await Context.Channel.SendMessageAsync(null, false, eb.Build());
        }

        public static async void splitUpLongMessageAsync(string msg, ISocketMessageChannel channel)
        {
            var charLimit = 1000;
            if(msg.Length >= charLimit )
            {
                var amtOfMsgs = Math.Ceiling((double)(msg.Length / charLimit));
                for(int i = 0; i < amtOfMsgs; i++)
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
    }
}
