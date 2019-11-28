using Discord.Commands;
using Discord.WebSocket;
using Neira.Web.Bot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Neira.Web.Bot.Services
{
	public class CommandHandlerService
	{
		private readonly IServiceProvider Services;
		private readonly DiscordSocketClient Client;
		private readonly LevelingService leveling;
		private CommandService Commands;

		public CommandHandlerService(IServiceProvider serviceProvider, DiscordSocketClient socketClient, CommandService commandService, LevelingService levelingService)
		{
			Services = serviceProvider;
			Client = socketClient;
			leveling = levelingService;
			Commands = commandService;
		}

		public async Task ConfigureAsync()
		{
			Commands = new CommandService(new CommandServiceConfig
			{
				DefaultRunMode = RunMode.Async,
				CaseSensitiveCommands = false
			});

			await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);
		}

		public async Task HandleCommandAsync(SocketMessage message)
		{
			// Ignore if not SocketUserMessage
			if (!(message is SocketUserMessage msg)) return;

			var context = new SocketCommandContext(Client, msg);
			var prefix = "!";
			if (msg.Channel is SocketGuildChannel)
			{
				//Get guild for load custom command Prefix.
				var config = await DatabaseHelper.GetGuildAccountAsync(context.Guild.Id);

				if (!string.IsNullOrWhiteSpace(config.CommandPrefix))
					prefix = config.CommandPrefix;
			}

			var argPos = 0;
			// Ignore if not mention this bot or command not start from prefix
			if (!(msg.HasMentionPrefix(Client.CurrentUser, ref argPos) || msg.HasStringPrefix(prefix, ref argPos))) return;
			{
				//search command
				var cmdSearchResult = Commands.Search(context, argPos);
				//If command not found just finish Task
				if (cmdSearchResult.Commands == null) return;
				//Execute command in current discord context
				var executionTask = Commands.ExecuteAsync(context, argPos, Services);

				await msg.DeleteAsync();

				await executionTask.ContinueWith(task =>
				{
					// If Success or command unknown just finish Task
					if (task.Result.IsSuccess || task.Result.Error == CommandError.UnknownCommand) return;

					context.Channel.SendMessageAsync($"{context.User.Mention} Ошибка: {task.Result.ErrorReason}");
				});
			}
			await Task.Run(async () =>
			{
				await leveling.GlobalLevel((SocketGuildUser)context.User);
				await leveling.MessageRewards((SocketGuildUser)context.User, message);
			});
		}

	}
}
