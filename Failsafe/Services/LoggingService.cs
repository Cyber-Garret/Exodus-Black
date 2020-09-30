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
		private readonly ILogger logger;
		private readonly DiscordSocketClient discord;
		private readonly CommandService command;


		public LoggingService(IServiceProvider services)
		{
			// get the services we need via DI, and assign the fields declared above to them
			discord = services.GetRequiredService<DiscordSocketClient>();
			command = services.GetRequiredService<CommandService>();
			logger = services.GetRequiredService<ILogger<LoggingService>>();

			discord.Log += OnLogAsync;
			command.Log += OnLogAsync;
		}

		// this method switches out the severity level from Discord.Net's API, and logs appropriately
		public Task OnLogAsync(LogMessage msg)
		{
			string logText = $"{msg.Source}: {msg.Message}";
			switch (msg.Severity.ToString())
			{
				case "Critical":
					{
						logger.LogCritical(logText);
						break;
					}
				case "Warning":
					{
						logger.LogWarning(logText);
						break;
					}
				case "Info":
					{
						logger.LogInformation(logText);
						break;
					}
				case "Verbose":
					{
						logger.LogInformation(logText);
						break;
					}
				case "Debug":
					{
						logger.LogDebug(logText);
						break;
					}
				case "Error":
					{
						logger.LogError(logText);
						break;
					}
				default:
					{
						logger.LogWarning(logText);
						break;
					}
			}

			return Task.CompletedTask;

		}
	}
}
