using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

namespace Failsafe.Services
{
	/// <summary>
	/// Provides logging for Discord.Net's API
	/// </summary>
	public class LoggingService
	{
		// declare the fields used later in this class
		private readonly ILogger<LoggingService> _logger;


		public LoggingService(ILogger<LoggingService> logger, IServiceProvider services)
		{
			_logger = logger;

			// get the services we need via DI, and assign the fields declared above to them
			var discord = services.GetRequiredService<DiscordSocketClient>();
			var command = services.GetRequiredService<CommandService>();

			discord.Log += OnLogAsync;
			command.Log += OnLogAsync;
		}

		// this method switches out the severity level from Discord.Net's API, and logs appropriately
		private Task OnLogAsync(LogMessage msg)
		{
			var logText = $"{msg.Source}: {msg.Message}";
			switch (msg.Severity.ToString())
			{
				case "Critical":
					_logger.LogCritical(logText);
					break;
				case "Warning":
					_logger.LogWarning(logText);
					break;
				case "Info":
				case "Verbose":
					_logger.LogInformation(logText);
					break;
				case "Debug":
					_logger.LogDebug(logText);
					break;
				case "Error":
					_logger.LogError(logText);
					break;
				default:
					_logger.LogWarning(logText);
					break;
			}

			return Task.CompletedTask;

		}
	}
}
