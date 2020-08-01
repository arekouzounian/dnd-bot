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
                await Context.Channel.SendMessageAsync($"You Rolled {num}.");
                return;
            }
            else
            {
                var nums = rollCode.Split('d');
                if(nums.Length > 2)
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
                if(rolls.Length > 1)
                {
                    await Context.Channel.SendMessageAsync(Format.Code($"{strB.ToString()}"));
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
            Random gen = new Random();
            var channel = Program.client.GetGuild(738549927537410048).GetChannel(738606355148963950);
            var users = Program.client.GetGuild(738549927537410048).Users;
            await (channel as ISocketMessageChannel).SendMessageAsync($"Time to roll for {Format.Bold("the stat!")}");
            foreach (var user in users)
            {
                if (user.IsBot)
                    continue;
                await (channel as ISocketMessageChannel).SendMessageAsync($"{user.Username}, you rolled {gen.Next(1, 21)} for {Format.Bold("the stat")} today.");
            }
        }

        [Command("spell")]
        public async Task findSpell(string spellName)
        {
            SpellHelper spelper = new SpellHelper(spellName);
            spelper.printSpell(Context);

        }

        //spells
        //monsters (requires admin)
        //races
        //classes

    }
}
