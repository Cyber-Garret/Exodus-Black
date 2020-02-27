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
		private readonly CommandService commands;
		public CommandHandlerService(IServiceProvider service)
		{
			this.service = service;
			config = service.GetRequiredService<IConfiguration>();
			discord = service.GetRequiredService<DiscordSocketClient>();
			commands = service.GetRequiredService<CommandService>();
		}

		public async Task InstallCommandsAsync()
		{
			// Hook the MessageReceived event into our command handler
			discord.MessageReceived += HandleCommandAsync;

			// Here we discover all of the command modules in the entry 
			// assembly and load them. Starting from Discord.NET 2.0, a
			// service provider is required to be passed into the
			// module registration method to inject the 
			// required dependencies.
			//
			// If you do not use Dependency Injection, pass null.
			// See Dependency Injection guide for more information.
			await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: service);
		}

		private Task HandleCommandAsync(SocketMessage message)
		{
			// ignore if not SocketUserMessage or author is bot
			if (!(message is SocketUserMessage msg) || message.Author.IsBot) return Task.CompletedTask;

			//New Task for fix disconeting from Discord WebSockets by 1001 if current Task not completed.
			Task.Run(async () =>
			 {
				 var context = new SocketCommandContext(discord, msg);

				 var argPos = 0;
				 // ignore if command not start from prefix
				 if (!msg.HasStringPrefix(config["Bot:Prefix"], ref argPos)) return;

				 // search command
				 var cmdSearchResult = commands.Search(context, argPos);
				 // if command not found just finish Task
				 if (cmdSearchResult.Commands == null) return;
				 //Execute command in current discord context
				 var executionTask = commands.ExecuteAsync(context, argPos, service);

				 await executionTask.ContinueWith(task =>
				 {
					 // if Success or command unknown just finish Task
					 if (task.Result.IsSuccess || task.Result.Error == CommandError.UnknownCommand) return;

					 context.Channel.SendMessageAsync($"{context.User.Mention} Ошибка: {task.Result.ErrorReason}");
				 });
			 });
			return Task.CompletedTask;
		}
	}
}
