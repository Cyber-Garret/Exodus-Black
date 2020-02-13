using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Bot.Services
{
	public class CommandHandlerService
	{
		private readonly IServiceProvider service;
		private readonly IConfiguration config;
		private readonly DiscordSocketClient discord;
		private CommandService command;
		public CommandHandlerService(IServiceProvider service)
		{
			this.service = service;
			config = service.GetRequiredService<IConfiguration>();
			discord = service.GetRequiredService<DiscordSocketClient>();
			command = service.GetRequiredService<CommandService>();
		}

		internal async Task ConfigureAsync()
		{
			command = new CommandService(new CommandServiceConfig
			{
				DefaultRunMode = RunMode.Async,
				CaseSensitiveCommands = false
			});
			await command.AddModulesAsync(Assembly.GetEntryAssembly(), service);
		}

		internal async Task HandleCommandAsync(SocketMessage message)
		{
			// ignore if not SocketUserMessage
			if (!(message is SocketUserMessage msg)) return;

			var context = new SocketCommandContext(discord, msg);

			var argPos = 0;
			// ignore if command not start from prefix
			if (!msg.HasStringPrefix(config["Bot:Prefix"], ref argPos)) return;

			// search command
			var cmdSearchResult = command.Search(context, argPos);
			// if command not found just finish Task
			if (cmdSearchResult.Commands == null) return;
			//Execute command in current discord context
			var executionTask = command.ExecuteAsync(context, argPos, service);

			await executionTask.ContinueWith(task =>
			{
				// if Success or command unknown just finish Task
				if (task.Result.IsSuccess || task.Result.Error == CommandError.UnknownCommand) return;

				context.Channel.SendMessageAsync($"{context.User.Mention} Ошибка: {task.Result.ErrorReason}");
			});
		}
	}
}
