using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Failsafe.Core.Data;
using Failsafe.Properties;

using Microsoft.Extensions.Configuration;
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

		private async Task OnMessageReceivedAsync(SocketMessage messageParam)
		{
			// Don't process the command if it was a system message or it's from bot
			if (messageParam is not SocketUserMessage message || messageParam.Author.IsBot) return;

			// Create a WebSocket-based command context based on the message
			var context = new SocketCommandContext(_discord, message);

			var guild = GuildData.GetGuildAccount(context.Guild);
			Thread.CurrentThread.CurrentUICulture = guild.Language;

			// Create a number to track where the prefix ends and the command begins
			int argPos = 0;

			// Determine if the message is a command based on the prefix and make sure no bots trigger commands
			if (!(message.HasStringPrefix(guild.CommandPrefix ?? _config["Bot:Prefix"], ref argPos) ||
				  message.HasMentionPrefix(_discord.CurrentUser, ref argPos)) ||
				message.Author.IsBot)
				return;

			// Execute the command with the command context we just
			// created, along with the service provider for precondition checks.
			await _commands.ExecuteAsync(
				context: context,
				argPos: argPos,
				services: _service);
		}

		private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
		{
			// We can tell the user what went wrong
			if (!result.IsSuccess)
			{
				await context.Channel.SendMessageAsync($"{context.User.Mention}, {Resources.ErrorHndlCom} {result.ErrorReason}");

				_logger.LogError("Error in command {commandName} by {errorValue}, with reason {errorResult}", command.Value.Name, result.Error!.Value, result.ErrorReason);
			}
		}
	}
}
