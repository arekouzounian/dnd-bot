using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace dnd_bot
{
    public class GetHelp
    {
        private CommandService _commands;
        public GetHelp(CommandService commands)
        {
            _commands = commands;
        }

        public EmbedBuilder GetHelpTextEmbed()
        {
            var eb = new EmbedBuilder();
            eb.WithTitle("Help");
            eb.AddField("How to use", "You can either use @dndbot#2832 or a / before each command, to let the bot know that you're executing a commmand.");
            eb.AddField("For example:", "You can say @dndbot#2832 roll 1d20, or you can say /roll 1d20.");
            foreach (var com in _commands.Commands)
            {
                if (com.Name.ToLower().Contains("stat"))
                    continue;
                eb.AddField($"{com.Name} ", $"{com.Summary}");
            }
            eb.WithFooter("Source: This bot was made by Arek Ouzounian, and its source code can be found here: https://github.com/arekouzounian/dnd-bot");

            return eb;
        }
    }
}
