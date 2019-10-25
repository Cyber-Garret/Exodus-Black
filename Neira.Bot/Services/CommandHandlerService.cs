using Discord.Commands;
using Discord.WebSocket;

using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Neira.Bot.Services
{
	public class CommandHandlerService
	{
		private readonly IServiceProvider Services;
		private readonly DiscordSocketClient Client;
		private readonly DbService db;
		private CommandService Commands;

		public CommandHandlerService(IServiceProvider serviceProvider, DiscordSocketClient socketClient, CommandService commandService, DbService dbService)
		{
			Services = serviceProvider;
			Client = socketClient;
			db = dbService;
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

		public async Task HandleCommandAsync(SocketMessage arg)
		{
			// Ignore if not SocketUserMessage or its direct message or private groups
			if (!(arg is SocketUserMessage msg)) return;
			//if (msg.Channel is SocketDMChannel || msg.Channel is SocketGroupChannel) return;

			var context = new SocketCommandContext(Client, msg);
			var prefix = "!";
			if(msg.Channel is SocketGuildChannel)
			{
				//Get guild for load custom command Prefix.
				var config = await db.GetGuildAccountAsync(context.Guild.Id);

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
				//Execute commant in current discord context
				var executionTask = Commands.ExecuteAsync(context, argPos, Services);

				await executionTask.ContinueWith(task =>
				 {
					 // If Success or command unknown just finish Task
					 if (task.Result.IsSuccess || task.Result.Error == CommandError.UnknownCommand) return;

					 context.Channel.SendMessageAsync($"{context.User.Mention} Ошибка: {task.Result.ErrorReason}");
				 });
			}
		}

	}
}
