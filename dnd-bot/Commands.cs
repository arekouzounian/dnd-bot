using Discord.Commands;
using System;
using Discord;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace dnd_bot
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        public WeaponHelper welper = new WeaponHelper();
        public theStatHandler statHandler = new theStatHandler();

        [Command("roll")]
        public async Task roll(string rollCode)
        {
            Random gen = new Random();
            await Context.Channel.SendMessageAsync($"Rolling {rollCode}...");
            int num;
            if(int.TryParse(rollCode, out num))
            {
                await Context.Channel.SendMessageAsync(Format.Code($"You Rolled {num}."));
                if(num == 17)
                {
                    await Context.Channel.SendMessageAsync(Format.Bold(Format.Italics("That doesn't hit.")));
                }
                return;
            }
            else
            {
                var nums = rollCode.Split('d');
                if(nums.Length != 2)
                {
                    await Context.Channel.SendMessageAsync($"That is an invalid amount of dice. The amount should look like this: [number of dice]d[number of sides on each given die]");
                    return;
                }
                if(int.Parse(nums[0]) > 500)
                {
                    await Context.Channel.SendMessageAsync("That is too many dice! You can only roll up to 500 dice at a time.");
                    return;
                }

                int[] rolls = new int[int.Parse(nums[0])]; 
                for(int i = 0; i < rolls.Length; i++)
                {
                    rolls[i] = gen.Next(1, int.Parse(nums[1]) + 1);
                }
                StringBuilder strB = new StringBuilder();
                int sum = 0;
                for(int i = 0; i < rolls.Length; i++)
                {
                    if(i == rolls.Length - 1)
                    {
                        strB.Append(rolls[i]);
                    }
                    else
                    {
                        strB.Append($"{rolls[i]} + ");
                    }
                    sum += rolls[i];
                }
                var charLimit = 1900;
                if(rolls.Length > 1)
                {
                    if(strB.ToString().Length > charLimit)
                    {
                        var amtOfMsgs = Math.Ceiling((double)(strB.Length / charLimit));    //(double)strB.ToString().Length / charLimit > (int)strB.ToString().Length / charLimit ? (int)strB.ToString().Length / charLimit + 1 : (int)strB.ToString().Length / charLimit;
                        for(int i = 0; i < amtOfMsgs; i++)
                        {
                            var substring = Format.Code(strB.ToString().Substring(i * charLimit, charLimit));
                            await Context.Channel.SendMessageAsync(substring);
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync(Format.Code($"{strB.ToString()}"));
                    }
                    
                }
                await Context.Channel.SendMessageAsync(Format.Code($"You Rolled {sum}."));

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
                            else if(roll == 20)
                            {
                                winCount++;
                            }
                        }
                        await Context.Channel.SendMessageAsync($"You rolled {winCount} natural 20s and {failCount} natural 1s!");
                    }
                }
            }
        }

        [Command("test")]
        [RequireOwner]
        public async Task test(string testString)
        {
            ;
        }

        [Command("spell")]
        public async Task findSpell(string spellName)
        {
            SpellHelper spelper = new SpellHelper(spellName);
            spelper.printSpell(Context);

        }

        [Command("monster")]
        //[RequireUserPermission(GuildPermission.Administrator)]
        public async Task findMonster(string monsterName)
        {
            MonsterHelper melper = new MonsterHelper(monsterName);
            melper.printMonster(Context);
            
        }

        [Command("help"), Alias("Help")]
        public async Task help()
        {
            var eb = getHelp.helpTextEmbed;
            await Context.Channel.SendMessageAsync(null, false, eb.Build());
        }

        [Command("roll for the stat")]
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
            if(weaponAdded)
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
            if(weaponList.Weapons.Count > 0)
            {
                EmbedBuilder eb = new EmbedBuilder();
                eb.WithTitle($"{Context.User.Username}'s Weapons");
                eb.WithColor(Color.DarkGrey);
                int weaponCount = 0;
                foreach(var weapon in weaponList.Weapons)
                {
                    if(weapon.OwnerID == Context.User.Id)
                    {
                        eb.AddField(weapon.Name, $"Damage Dice: {weapon.DamageDice}" +
                        $"\nDamage Type: {weapon.DamageType}" +
                        $"\nEffects: {weapon.Effects}");
                        weaponCount++;
                    }
                }
                if(weaponCount == 0)
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
        public async Task rollDamage(string weaponName)
        {
            bool weaponRolled = false;
            foreach(var weapon in welper.GetWeapons().Weapons)
            {
                if (weapon.Name.ToLower() == weaponName.ToLower() && weapon.OwnerID == Context.User.Id)
                {
                    await roll(weapon.DamageDice);
                    weaponRolled = true;
                }
            }
            if(!weaponRolled)
            {
                await Context.Channel.SendMessageAsync("You don't have that weapon.");
            }
        }

        [Command("statAvg"), Alias("statAnvg")]
        public async Task getStatAvg()
        {
            if(statHandler.getCareerAvg(Context.User.Id) == -1)
            {
                await Context.Channel.SendMessageAsync($"I don't have your career average stored yet. Once {Format.Bold("the stat")} has been rolled more, I can compute your average.");
            }
            else
            {
                await Context.Channel.SendMessageAsync($"Your career average for {Format.Bold("the stat")} is: {statHandler.getCareerAvg(Context.User.Id)}.");
            }
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

        [Command("dmMode")]
        [RequireOwner]
        public async Task DmMode()
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            bool isDone = false;

            while(!isDone)
            {
                var msg = Console.ReadLine();

                if(msg == "terminate")
                {
                    isDone = true;
                    continue;
                }
                else if(msg == "")
                {
                    continue;
                }
                else
                {
                    await Context.Channel.SendMessageAsync(msg);
                }
            }
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
    }
}
