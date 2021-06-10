﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Failsafe.Core;
using Failsafe.Core.Data;
using Failsafe.Modules;
using Failsafe.Properties;

using Microsoft.Extensions.Configuration;

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Failsafe.Models;
using Microsoft.Extensions.Logging;

namespace Failsafe.Services
{
    public class CommandHandlerService
    {
        private readonly ILogger<CommandHandlerService> _logger;
        private readonly IServiceProvider _service;
        private readonly IConfiguration _config;
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;

        public CommandHandlerService(ILogger<CommandHandlerService> logger, IServiceProvider service, IConfiguration config, DiscordSocketClient discord, CommandService commands)
        {
            _logger = logger;
            _service = service;
            _config = config;
            _discord = discord;
            _commands = commands;
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            _discord.MessageReceived += OnMessageReceivedAsync;
            _commands.CommandExecuted += OnCommandExecutedAsync;

            // Here we discover all of the command modules in the entry
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _service);
        }

        private Task OnMessageReceivedAsync(SocketMessage message)
        {
            // ignore if not SocketUserMessage or author is bot
            if (!message.IsA<SocketUserMessage>() || message.Author.IsBot) return Task.CompletedTask;

            var msg = (SocketUserMessage)message;
            //New Task for fix disconnecting from Discord WebSockets by 1001 if current Task not completed.
            Task.Run(async () =>
             {
                 try
                 {
                     var context = new SocketCommandContext(_discord, msg);
                     var guild = GuildData.GetGuildAccount(context.Guild);
                     Thread.CurrentThread.CurrentUICulture = guild.Language;
                     var argPos = 0;
                     // ignore if command not start from prefix
                     var prefix = guild.CommandPrefix ?? _config["Bot:Prefix"];
                     if (msg.HasStringPrefix(prefix, ref argPos) || msg.HasMentionPrefix(_discord.CurrentUser, ref argPos))
                     {
                         // search command
                         var cmdSearchResult = _commands.Search(context, argPos);
                         // if command not found just finish Task
                         if (cmdSearchResult.Commands == null) return;
                         //Execute command in current discord context
                         await _commands.ExecuteAsync(context, argPos, _service);
                     }
                 }
                 catch (Exception e)
                 {
                     _logger.LogError(e, "Error in HandleCommandAsync");
                 }

             });
            return Task.CompletedTask;
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // We can tell the user what went wrong
            if (!string.IsNullOrEmpty(result?.ErrorReason))
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention}, {Resources.ErrorHndlCom} {result.ErrorReason}");
            }

            if (command.IsSpecified)
            {
                CommandStatisticData.TryUpdateStat(new CommandStat
                {
                    Name = command.Value.Aliases[1],
                    Count = 1,
                    Date = DateTime.Now.ToShortDateString()
                });
            }
        }
    }
}
