using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Failsafe.Core;
using Failsafe.Core.Data;
using Failsafe.Models;
using Failsafe.Properties;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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

		private async Task OnMessageReceivedAsync(SocketMessage messageParam)
		{
			// Don't process the command if it was a system message or it's from bot
			if (messageParam is not SocketUserMessage message || messageParam.Author.IsBot) return;

			// Create a number to track where the prefix ends and the command begins
			int argPos = 0;

			// Determine if the message is a command based on the prefix and make sure no bots trigger commands
			if (!(message.HasCharPrefix('!', ref argPos) ||
				  message.HasMentionPrefix(_discord.CurrentUser, ref argPos)) ||
				message.Author.IsBot)
				return;

			// Create a WebSocket-based command context based on the message
			var context = new SocketCommandContext(_discord, message);

			// Execute the command with the command context we just
			// created, along with the service provider for precondition checks.
			await _commands.ExecuteAsync(
				context: context,
				argPos: argPos,
				services: null);

			//// ignore if not SocketUserMessage or author is bot
			//if (!message.IsA<SocketUserMessage>() || message.Author.IsBot) return Task.CompletedTask;

			//var msg = (SocketUserMessage)message;
			////New Task for fix disconnecting from Discord WebSockets by 1001 if current Task not completed.
			//Task.Run(async () =>
			// {
			//	 try
			//	 {
			//		 var context = new SocketCommandContext(_discord, msg);
			//		 var guild = GuildData.GetGuildAccount(context.Guild);
			//		 Thread.CurrentThread.CurrentUICulture = guild.Language;
			//		 var argPos = 0;
			//		 // ignore if command not start from prefix
			//		 var prefix = guild.CommandPrefix ?? _config["Bot:Prefix"];
			//		 if (msg.HasStringPrefix(prefix, ref argPos) || msg.HasMentionPrefix(_discord.CurrentUser, ref argPos))
			//		 {
			//			 // search command
			//			 var cmdSearchResult = _commands.Search(context, argPos);
			//			 // if command not found just finish Task
			//			 if (cmdSearchResult.Commands == null) return;
			//			 //Execute command in current discord context
			//			 await _commands.ExecuteAsync(context, argPos, _service);
			//		 }
			//	 }
			//	 catch (Exception e)
			//	 {
			//		 _logger.LogError(e, "Error in HandleCommandAsync");
			//	 }

			// });
			//return Task.CompletedTask;
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
