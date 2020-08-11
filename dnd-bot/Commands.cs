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
        public async Task test()
        {
            await Context.Channel.SendMessageAsync("Str\tDex\tCha\tInt\tWis");
            await Context.Channel.SendMessageAsync("10\t10\t10\t10\t10");
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
            Program.rollForTheStat();
        }

        //spells - 
        //monsters (requires admin) - 
        //races
        //classes

    }
}
