using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Neuromatrix.Services
{
    public struct Logger
    {
        #region Private fields
        private readonly string _source;
        private readonly LogService _log;
        #endregion

        public Logger(string source, LogService log)
        {
            _source = source;
            _log = log;
        }

        public Task InfoAsync(string message)
            => _log.LogAsync(new LogMessage(LogSeverity.Info, _source, message));

        public Task WarnAsync(string message, Exception ex = null)
            => _log.LogAsync(new LogMessage(LogSeverity.Warning, _source, message, ex));
    }

    public class LogService
    {
        #region Private fields
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        #endregion

        public LogService(CommandService commands, DiscordSocketClient discord)
        {
            _discord = discord;
            _commands = commands;
        }

        public void Configure()
        {
            _commands.Log += LogAsync;
            _discord.Log += LogAsync;
        }

        public Task LogAsync(LogMessage message)
        {
            Console.WriteLine($"[{DateTime.Now} Source: {message.Source}] Message: {message.Message}");
            return Task.CompletedTask;
        }
    }
}
