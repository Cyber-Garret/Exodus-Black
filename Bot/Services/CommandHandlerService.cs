using Discord;
using Discord.Commands;
using Discord.WebSocket;

using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Bot.Services
{
	public class CommandHandlerService
	{
		#region Private fields
		private readonly DiscordSocketClient Client;
		private CommandService Commands;
		private readonly IServiceProvider Services;
		#endregion

		public CommandHandlerService(IServiceProvider serviceProvider, DiscordSocketClient socketClient, CommandService commandService)
		{
			Services = serviceProvider;
			Client = socketClient;
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
			// Ignore if not SocketUserMessage or its Direct Message
			if (!(arg is SocketUserMessage msg)) return;
			if (msg.Channel is SocketDMChannel) return;

			var context = new SocketCommandContext(Client, msg);
			if (context.User.IsBot) return;

			var config = await FailsafeDbOperations.GetGuildAccountAsync(context.Guild.Id);
			var prefix = config.CommandPrefix ?? "!";


			var argPos = 0;
			// Ignore if not mention this bot or command not start from char !
			if (!(msg.HasMentionPrefix(Client.CurrentUser, ref argPos) || msg.HasStringPrefix(prefix, ref argPos))) return;
			{

				var cmdSearchResult = Commands.Search(context, argPos);
				if (cmdSearchResult.Commands == null) return;

				var executionTask = Commands.ExecuteAsync(context, argPos, Services);

				await executionTask.ContinueWith(task =>
				 {
					 if (task.Result.IsSuccess || task.Result.Error == CommandError.UnknownCommand) return;
					 const string errTemplate = "{0}, {1}.";
					 var errMessage = string.Format(errTemplate, context.User.Mention, task.Result.ErrorReason);
					 context.Channel.SendMessageAsync(errMessage);
				 });
			}
		}

	}
}
