using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Neuromatrix.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Neuromatrix.Services
{
    public class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly LogService _log;
        private readonly RateLimitService _rateLimitService;
        private readonly IServiceProvider _services;

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
            if (msg.Author.IsBot) return;

            int argPos = 0;
            if (!(msg.HasMentionPrefix(_discord.CurrentUser, ref argPos)
                || msg.HasCharPrefix('!', ref argPos))) return;

            var context = new SocketCommandContext(_discord, msg);
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        //private async Task Client_MessageReceived(SocketMessage MessageParams)
        //{
        //    var Message = MessageParams as SocketUserMessage;
        //    var Context = new SocketCommandContext(Client, Message);

        //    //Ignore private message.
        //    if (Message.Channel is SocketDMChannel) return;
        //    //Ignore if message null or empty content.
        //    if (Context.Message == null || Context.Message.Content == "") return;
        //    //Ignore message from bot.
        //    if (Context.User.IsBot) return;

        //    int ArgPos = 0;
        //    if (!(Message.HasStringPrefix("!", ref ArgPos) || Message.HasMentionPrefix(Client.CurrentUser, ref ArgPos))) return;

        //    var Result = await Command.ExecuteAsync(Context, ArgPos);
        //    if (!Result.IsSuccess)
        //        Console.WriteLine($"[{DateTime.Now} in command ] Sometimes went wrong with commands. Text: {Context.Message.Content} | Error: {Result.ErrorReason}");
        //}

        public async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified) return; // ignore search failures
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ToString());
            else
            {
                var limits = command.Value.Preconditions.OfType<RateLimitAttribute>();
                foreach (var limit in limits)
                {
                    var rule = limit.GetRule(_rateLimitService, context, command.Value);
                    //safe to pass nil here, this factory would have been run in the precondition earlier
                    _rateLimitService.GetOrAdd(rule, BogusFactory).Increment();
                }
            }
            var log = new LogMessage(LogSeverity.Info, "chs", $"{context.User} invoked {command.Value.Name} in {context.Channel} with {result}");
            await _log.LogAsync(log);
        }
        private RateLimitInfo BogusFactory(string _)
            => throw new InvalidOperationException("This shouldn't be happening in the first place!!");
    }
}
