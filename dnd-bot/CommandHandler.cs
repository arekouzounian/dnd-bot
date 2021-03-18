using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace dnd_bot
{
    class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        // Retrieve client and CommandService instance via ctor
        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services)
        {
            _commands = commands;
            _client = client;
            _services = services;
        }

        public async Task SetupAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            // Hook the execution event
            _commands.CommandExecuted += OnCommandExecutedAsync;
            // Hook the command handler
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!string.IsNullOrEmpty(result?.ErrorReason))
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }

            var commandName = command.IsSpecified ? command.Value.Name : "A command";
            /*await _log.LogAsync(new LogMessage(LogSeverity.Info,
                "CommandExecution",
                $"{commandName} was executed at {DateTime.UtcNow}."));*/
            Console.WriteLine($"CommandExecution] {commandName} was executed at {DateTime.UtcNow}.");
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            var Context = new SocketCommandContext(_client, message);

            if (message.Author == Context.Guild.Owner && message.Content.ToLower().Contains("am i right"))
            {
                await Context.Channel.SendMessageAsync("he's right");
            }

            int argPos = 0;
            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!message.HasMentionPrefix(_client.CurrentUser, ref argPos) && !message.HasCharPrefix('/', ref argPos))
            {
                return;
            }
            if (message.Author.IsBot)
                return;

            var result = await _commands.ExecuteAsync(
                context: Context,
                argPos: argPos,
                services: _services);
        }

    }
}
