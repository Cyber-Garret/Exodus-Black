using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using Nett;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using DiscordBot.Models;
using DiscordBot.Services;

namespace DiscordBot
{
	class Program
	{
		public static DiscordShardedClient Client;

		#region Private fields
		private IServiceProvider _services;
		private static string _config_path;
		private readonly int[] _shardIds = { 0, 1 };
		#endregion


		private static void Main()
		{
			Console.Title = $"Neuromatrix Discord Bot (Discord.Net v{DiscordConfig.Version})";
			_config_path = Directory.GetCurrentDirectory() + "/UserData/config.tml";

			new Program().StartAsync().GetAwaiter().GetResult();
		}

		private async Task StartAsync()
		{
			Client = new DiscordShardedClient(_shardIds, new DiscordSocketConfig
			{
				LogLevel = LogSeverity.Verbose,
				DefaultRetryMode = RetryMode.AlwaysRetry,
				MessageCacheSize = 100,
				TotalShards = 2
			});

			Client.Log += Logger.Log;

			#region Configure services
			_services = BuildServices();

			_services.GetRequiredService<ConfigurationService>().Configure();
			_services.GetRequiredService<ReminderService>().Configure();
			_services.GetRequiredService<DiscordEventHandlerService>().Configure();
			await _services.GetRequiredService<CommandHandlerService>().ConfigureAsync();
			#endregion

			var token = _services.GetRequiredService<Configuration>().Token;


			await Client.LoginAsync(TokenType.Bot, token);
			await Client.StartAsync();


			await Client.SetGameAsync("!справка");
			await Client.SetStatusAsync(UserStatus.Online);

			await Task.Delay(-1);
		}

		public ServiceProvider BuildServices()
		{
			return new ServiceCollection()
				.AddSingleton(_ => Toml.ReadFile<Configuration>(_config_path))
				.AddSingleton<ConfigurationService>()
				.AddSingleton<CommandService>()
				.AddSingleton<CommandHandlerService>()
				.AddSingleton<DiscordEventHandlerService>()
				.AddSingleton<ReminderService>()
				.BuildServiceProvider();
		}

	}
}
