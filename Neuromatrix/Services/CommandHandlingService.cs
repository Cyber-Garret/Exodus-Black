using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Neuromatrix.Preconditions;

namespace Neuromatrix.Services
{
    public class CommandHandlingService
    {
        #region Private fields
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly LogService _log;
        private readonly RateLimitService _rateLimitService;
        private readonly IServiceProvider _services;
        #endregion

        public CommandHandlingService(CommandService commands,
            DiscordSocketClient discord,
            LogService log,
            RateLimitService rateLimit,
            IServiceProvider services)
        {
            _commands = commands;
            _discord = discord;
            _log = log;
            _rateLimitService = rateLimit;
            _services = services;

            _commands.CommandExecuted += OnCommandExecutedAsync;
            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        public async Task ConfigureAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task OnMessageReceivedAsync(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage msg)) return;
            if (msg.Author.IsBot) return; // Ignore message from other bot
            //if (msg.Channel is SocketDMChannel) return; // Ignore PM messages.

            int argPos = 0;
            if (!(msg.HasMentionPrefix(_discord.CurrentUser, ref argPos)
                || msg.HasCharPrefix('!', ref argPos))) return;

            var context = new SocketCommandContext(_discord, msg);
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified) return; // Ignore search failures
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ToString());
            else
            {
                var limits = command.Value.Preconditions.OfType<RateLimitAttribute>();
                foreach (var limit in limits)
                {
                    var rule = limit.GetRule(_rateLimitService, context, command.Value);
                    //Safe to pass null here, this factory would have been run in the precondition earlier
                    _rateLimitService.GetOrAdd(rule, BogusFactory).Increment();
                }
            }
            var log = new LogMessage(LogSeverity.Info, "chs", $"{context.User} invoked command {command.Value.Name} in channel {context.Channel} with result {result}");
            await _log.LogAsync(log);
        }

        private RateLimitInfo BogusFactory(string _)
            => throw new InvalidOperationException("This shouldn't be happening in the first place!!");
    }
}
