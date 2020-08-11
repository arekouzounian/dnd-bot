# dndbot
A personalized bot for my D&amp;D server!

dndbot was specifically created for my D&D discord server, which is my some features might not make much sense out-of-context.
It is for the same reason that a lot of the code checks for users and guild by use of their Id's, rather than being more generic.
I don't plan on having dndbot be on any other servers.

Current Features:
- Roll Command: using the command format "[botmention] roll [amount of dice]d[amount of sides each individual die has]," the bot will roll 
    however many dice of however many sides you like, up to a maximum of 500 dice. The bot will then show the sum of the die rolls!
    If you're rolling d20s, the bot also tells you how many critical successes and/or failures you rolled.
- Greeting Feature: upon joining the server, the bot will greet you in a private DM, showcasing its command syntax as well as what commands the user can use.
- #the-stat channel: Every day, the bot will roll "the stat" for each member of the server, and tell them what they rolled in the corresponding #the-stat text channel.
- 'spell' command: using the 'spell' command, the bot will lookup the specified spell using the 'dnd5eapi.co' API, and output all the information about the specified spell to the channel.
- 'monster' command: much like the 'spell' command, thebot will use the same API to look up the given monster, and output relevant details about the monster to the text channel.
- More features will come as soon as I can think of them.
- Cloud Deployment: This bot currently is deployed on a remote cloud server so that it is always online. 
    
Possible Future Features:
 - Scheduling Help: A command used in the #scheduling chat that DMs everyone to notify them of a new session being scheduled
 - Lookup feature: A command that uses an online database in order to look-up spells and items. 
