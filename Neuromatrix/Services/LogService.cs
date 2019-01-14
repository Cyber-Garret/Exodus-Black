using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Neuromatrix.Resources;

namespace Neuromatrix.Services
{
    public struct Logger
    {
        private readonly string source;
        private readonly LogService log;

        public Logger(string source, LogService log)
        {
            this.source = source;
            this.log = log;
        }

        public Task InfoAsync(string message)
            => log.LogAsync(new LogMessage(LogSeverity.Info, source, message));
        public Task WarnAsync(string message, Exception ex = null)
            => log.LogAsync(new LogMessage(LogSeverity.Warning, source, message, ex));
    }

    public class LogService
    {
        ServiceProvider service { get; set; }

        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;

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
            try
            {
                SocketGuild Guild = _discord.Guilds.Where(x => x.Id == service.GetRequiredService<Settings>().Guild).First();
                SocketTextChannel TextChannel = Guild.Channels.Where(x => x.Id == service.GetRequiredService<Settings>().LogChannel).First() as SocketTextChannel;
                TextChannel.SendMessageAsync($"[{DateTime.Now} Источник: {message.Source}] Сообщение: {message.Message}");
            }
            catch { }
            return Task.CompletedTask;
        }
    }
}
