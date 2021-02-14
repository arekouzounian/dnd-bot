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
                eb.AddField("How to use", "You can either use @dndbot#2832 or a / before each command, to let the bot know that you're executing a commmand.");
                eb.AddField("For example:", "You can say @dndbot#2832 roll 1d20, or you can say /roll 1d20.");
                eb.AddField("Commands", "Roll: the 'roll' command is used to roll dice. With this command, you could roll any number of dice (to a maximum of 500), of any type. " +
                    "So, for example, you could say '@dndbot#2832 roll 8d6', and the result will be a randomly rolled set of 8 d6s. In general, you want to follow this format: [number of dice rolled]d[number of faces on each individual die]." +
                    "You can also add modifiers to the end of you rolls. For example, you can do the following: /roll 1d20 + 3, or /roll 1d4 * 4. Any amount of modifiers can be used!" +
                    "\nSpell: this command looks through the available 5e SRD to look up the specified spell. Though this feature has access to over 300 spells, it is by no means perfect and doesn't have every spell. " +
                    "To use it, simply type '/spell {spell name}'" +
                    "\nMonster: this command, similar to the 'spell' command, looks through the 5e SRD to look up the specified monster. Again, it is limited to whatever the SRD has, so it won't have a lot of monsters." +
                    "To use this command, simply type '@dndbot#2832 monster {monster name}'");
                eb.AddField("Upcoming Features", "Scheduling! The bot will send out a message in the #session-scheduling channel, where people will be able to RSVP via a reaction to the message. The bot will keep track of who can and can't come, and will relay that information to whoever can host!");
                eb.WithFooter("Source: This bot was made by Arek Ouzounian, and its source code can be found here: https://github.com/arekouzounian/dnd-bot");

                return eb;
            }
            private set { }
        }
    }
}
