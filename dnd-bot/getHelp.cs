using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace dnd_bot
{
    public class getHelp
    {
        public getHelp() { }

        public static EmbedBuilder helpTextEmbed
        {
            get
            {
                var eb = new EmbedBuilder();
                eb.WithTitle("Help");
                eb.AddField("How to use", "My commands follow a distinct syntax: '@dndbot#2832[space]{command}[space]{command parameter}'");
                eb.AddField("For example:", "@dndbot#2832 roll 1d20");
                eb.AddField("Commands", "Roll: the 'roll' command is used to roll dice. With this command, you could roll any number of dice (to a maximum of 500), of any type. " +
                    "So, for example, you could say '@dndbot$2832 roll 8d6', and the result will be a randomly rolled set of 8 d6s. In general, you want to follow this format: [number of dice rolled]d[number of faces on each individual die]" +
                    "\nSpell: this command looks through the available 5e SRD to look up the specified spell. Though this feature has access to over 300 spells, it is by no means perfect and doesn't have every spell. " +
                    "To use it, simply type '@dndbot#2832 spell {spell name}'" +
                    "\nMonster: this command, similar to the 'spell' command, looks through the 5e SRD to look up the specified monster. Again, it is limited to whatever the SRD has, so it won't have a lot of monsters." +
                    "To use this command, simply type '@dndbot#2832 monster {monster name}'");
                eb.AddField("Troubleshooting", "Whenever a command parameter is more than one word, make sure to put it in quotes. So for example: " +
                    "'@dndbot#2832 spell mimic' is acceptable, because 'Mimic' is only one word. But if you wanted to look up something like an Ancient White Dragon, you would say: " +
                    "'@dndbot#2832 monster \"Ancient White Dragon\"'");
                eb.AddField("Source", "This bot was made by Arek Ouzounian, and its source code can be found here: https://github.com/arekouzounian/dnd-bot");

                return eb;
            }
            private set { }
        }
    }
}
