using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

namespace Bot.Services
{
	/// <summary>
	/// Provides logging for Discord.Net's API
	/// </summary>
	public class LoggingService
	{
		// declare the fields used later in this class
		private readonly ILogger logger;
		private readonly DiscordSocketClient discord;
		private readonly CommandService command;

		public LoggingService(IServiceProvider services)
		{
			// get the services we need via DI, and assign the fields declared above to them
			discord = services.GetRequiredService<DiscordSocketClient>();
			command = services.GetRequiredService<CommandService>();
			logger = services.GetRequiredService<ILogger<LoggingService>>();
		}

		public void Configure()
		{
			// hook into these events with the methods provided below
			discord.Ready += OnReadyAsync;
			discord.Log += OnLogAsync;
			discord.Disconnected += OnDisconnectedAsync;
			command.Log += OnLogAsync;
		}

		// this method executes on the bot being connected/ready
		private Task OnReadyAsync()
		{
			logger.LogInformation($"Connected as -> [{discord.CurrentUser}] :)");
			return Task.CompletedTask;
		}

		// this method executes on the bot being disconnected from Discord API
		private Task OnDisconnectedAsync(Exception ex)
		{
			logger.LogInformation($"Bot disconnected. [{ex.Message}]");
			return Task.CompletedTask;
		}

		// this method switches out the severity level from Discord.Net's API, and logs appropriately
		private Task OnLogAsync(LogMessage message)
		{
			string logText = $"{message.Source}: {message.Message}";
			switch (message.Severity)
			{
				case LogSeverity.Critical:
					{
						logger.LogCritical(logText);
						break;
					}
				case LogSeverity.Error:
					{
						logger.LogError(logText);
						break;
					}
				case LogSeverity.Warning:
					{
						logger.LogWarning(logText);
						break;
					}
				case LogSeverity.Info:
					{
						logger.LogInformation(logText);
						break;
					}
				case LogSeverity.Verbose:
					{
						logger.LogTrace(logText);
						break;
					}
				case LogSeverity.Debug:
					{
						logger.LogDebug(logText);
						break;
					}

				default:
					logger.LogWarning(logText);
					break;
			}
			return Task.CompletedTask;
		}
	}
}
