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
using System.Runtime.Loader;
using System.Threading.Tasks;

using Victoria;

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
			AssemblyLoadContext.Default.Unloading += SigTermEventHandler; //register sigterm event handler.
			Console.CancelKeyPress += CancelHandler; //register sigint event handler

			config = GetBotSettings();

			Console.Title = $"{config.BotName} Discord Bot (Discord.NET v{DiscordConfig.Version})";

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


			await Discord.LoginAsync(TokenType.Bot, config.Discord.BotToken);
			await Discord.StartAsync();
			await Discord.SetStatusAsync(UserStatus.Online);

			await Task.Delay(-1);
		}



		#region Program events
		private static void SigTermEventHandler(AssemblyLoadContext obj)
		{
			Console.WriteLine("Unloading...");
		}

		private static void CancelHandler(object sender, ConsoleCancelEventArgs e)
		{
			Console.WriteLine("Exiting...");
		}
		#endregion

		#region Functions
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
					AlwaysDownloadUsers = true,
					LogLevel = LogSeverity.Info,
					DefaultRetryMode = RetryMode.AlwaysRetry,
					MessageCacheSize = 100
				}))
				.AddDbContext<NeiraContext>()
				.AddSingleton<CommandService>()
				.AddSingleton<CommandHandlerService>()
				.AddSingleton<DiscordEventHandlerService>()
				.AddSingleton<InteractiveService>()
				.AddSingleton<TimerService>()
				.AddSingleton<MilestoneService>()
				.AddSingleton<MusicService>()
				.AddSingleton<LavaRestClient>()
				.AddSingleton<LavaSocketClient>()
				.BuildServiceProvider();
		}
		#endregion


	}
}
