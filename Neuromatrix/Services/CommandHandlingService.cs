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
        private DiscordShardedClient _client;
        private CommandService _commands;
        private readonly IServiceProvider _services;
        #endregion

        public CommandHandlingService(DiscordShardedClient client, CommandService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task ConfigureAsync()
        {
            _commands = new CommandService();
            var cmdConfig = new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async
            };

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            Global.Client = _client;
        }

        public async Task HandleCommandAsync(SocketMessage arg)
        {

            if (!(arg is SocketUserMessage msg)) return;
            if (msg.Channel is SocketDMChannel) return;

            var context = new ShardedCommandContext(_client, msg);
            if (context.User.IsBot) return;
            

            var argPos = 0;
            if (!(msg.HasMentionPrefix(_client.CurrentUser, ref argPos) || msg.HasCharPrefix('!', ref argPos))) return;
            {

                var cmdSearchResult = _commands.Search(context, argPos);
                if (cmdSearchResult.Commands.Count == 0) await context.Channel.SendMessageAsync($"{context.User.Mention}, это неизвестная мне команда.");

                var executionTask = _commands.ExecuteAsync(context, argPos, _services);

                await executionTask.ContinueWith(task =>
                 {
                     if (task.Result.IsSuccess || task.Result.Error == CommandError.UnknownCommand) return;
                     const string errTemplate = "{0}, Ошибка: {1}.";
                     var errMessage = string.Format(errTemplate, context.User.Mention, task.Result.ErrorReason);
                     context.Channel.SendMessageAsync(errMessage);
                 });
            }
        }

    }
}
