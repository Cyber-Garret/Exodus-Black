using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Neira.Bot.Models;
using Neira.Bot.Services;
using Neira.Db;

using Nett;

using System;
using System.IO;
using System.Threading.Tasks;

namespace Neira.Bot
{
	class Program
	{
		private const string userPath = "UserData";
		private const string fileName = "config.toml";

		private IServiceProvider service;
		internal static BotSettings config;


		private static void Main()
		{
			config = GetBotSettings();

			Console.Title = $"Neira Bot (Discord.NET v{DiscordConfig.Version})";

			try
			{
				new Program().StartAsync().GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}, Source: Neira Main] Message: {ex.Message}");
			}
		}

		private async Task StartAsync()
		{
			service = BuildServices();

			var Discord = service.GetRequiredService<DiscordSocketClient>();
			Discord.Log += Logger.Log;

			service.GetRequiredService<TimerService>().Configure();
			service.GetRequiredService<DiscordEventHandlerService>().Configure();
			await service.GetRequiredService<CommandHandlerService>().ConfigureAsync();


			await Discord.LoginAsync(TokenType.Bot, config.BotToken);
			await Discord.StartAsync();
			await Discord.SetStatusAsync(UserStatus.Online);
			await Discord.SetGameAsync(@"http://neira.su/");

			await Task.Delay(-1);
		}
		private static BotSettings GetBotSettings()
		{
			try
			{
				return Toml.ReadFile<BotSettings>(Path.Combine(Directory.GetCurrentDirectory(), userPath, fileName));
			}
			catch
			{
				var initializeConfig = new BotSettings();
				Toml.WriteFile(initializeConfig, Path.Combine(userPath, fileName));
				return Toml.ReadFile<BotSettings>(Path.Combine(Directory.GetCurrentDirectory(), userPath, fileName));
			}
		}

		public ServiceProvider BuildServices()
		{
			return new ServiceCollection()
				.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
				{
					ExclusiveBulkDelete = true,
					AlwaysDownloadUsers = true,
					LogLevel = LogSeverity.Info,
					DefaultRetryMode = RetryMode.AlwaysRetry,
					MessageCacheSize = 300
				}))
				.AddDbContext<NeiraContext>()
				.AddSingleton<CommandService>()
				.AddSingleton<CommandHandlerService>()
				.AddSingleton<DiscordEventHandlerService>()
				.AddSingleton<InteractiveService>()
				.AddSingleton<TimerService>()
				.AddSingleton<MilestoneService>()
				.BuildServiceProvider();
		}


	}
}
