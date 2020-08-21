using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace dnd_bot
{
    class Program
    {
        public static DiscordSocketClient client;
        public CommandService Commands;
        private IServiceProvider services;
        public static SocketCommandContext gContext;

        public Random gen = new Random();
        public bool statRolled = true;
        public theStatHandler statHandler = new theStatHandler();


        static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100
            });

            Commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = true,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Debug
            });

            services = new ServiceCollection()
                .AddSingleton(this)
                .AddSingleton(client)
                .AddSingleton(Commands)
                .AddSingleton<ConfigHandler>()
                .BuildServiceProvider();

            await services.GetService<ConfigHandler>().FillConfig();

            client.MessageReceived += Client_MessageReceived;
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);

            client.Log += Client_Log;
            client.Ready += Client_Ready;
            client.UserJoined += Client_UserJoined;

            await client.LoginAsync(TokenType.Bot, services.GetService<ConfigHandler>().GetToken());
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task Client_UserJoined(SocketGuildUser user)
        {
            var playerRole = client.GetGuild(738549927537410048).GetRole(738556835036135467);
            await user.AddRoleAsync(playerRole);
            await user.SendMessageAsync(Format.Bold("Hello! Welcome to the server. I'm dndbot, and I was made specifically for this server."));
            var eb = getHelp.helpTextEmbed;
            await user.SendMessageAsync(null, false, eb.Build());
        }

        private async Task Client_Ready()
        {
            await client.SetGameAsync("Dungeons & Dragons");
        }

        private async Task Client_Log(LogMessage arg)
        {
            Console.WriteLine($"{DateTime.Now} at {arg.Source}] {arg.Message}");
            if (DateTime.Now.Hour >= 13 && DateTime.Now.Hour < 14 && !statRolled)
            {
                statHandler.rollForTheStat();
                statRolled = true;
            }
            else if (DateTime.Now.Hour >= 1 && DateTime.Now.Hour < 2)
            {
                statRolled = false;
            }
        }

        private async Task Client_MessageReceived(SocketMessage MessageParam)
        {
            var Message = MessageParam as SocketUserMessage;
            var Context = new SocketCommandContext(client, Message);
            gContext = Context;

            if (Context.Message == null || Context.Message.Content == "") return;
            if (Context.Message.Author.IsBot) return;

            int ArgPos = 0;
            //bool bruh = Message.HasCharPrefix('!', ref ArgPos);
            if (!Message.HasMentionPrefix(client.CurrentUser, ref ArgPos) && !Message.HasCharPrefix('/', ref ArgPos))
            {
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            var result = await Commands.ExecuteAsync(Context, ArgPos, services);

            if (!result.IsSuccess)
            {
                Console.WriteLine($"{DateTime.Now} at Commands] Something went wrong with executing a command. Text: {Context.Message.Content} | Error: {result.ErrorReason}");
                if (result.ErrorReason == "User requires guild permission Administrator")
                {
                    await Context.Channel.SendMessageAsync("Only admins can use that command!");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("That's not a command.");
                }
            }

        }
    }

}
