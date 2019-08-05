using Core;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using DiscordBot.Models;
using DiscordBot.Services;

using Microsoft.Extensions.DependencyInjection;

using Nett;

using System;
using System.IO;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Discord.Addons.Interactive;

namespace DiscordBot
{
	class Program
	{
		private const string userPath = "UserData";
		private const string fileName = "config.toml";

		private IServiceProvider service;
		private static BotSettings config;


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
				Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}, Source: Main] Message: {ex.Message}");
				//Not need for Lunix Unit service, but useful for debug session =)
#if DEBUG
				Console.WriteLine("Press any key to exit...");
				Console.ReadKey();
#endif
			}
		}

		private async Task StartAsync()
		{
			service = BuildServices();

			var shardedClient = service.GetRequiredService<DiscordShardedClient>();
			shardedClient.Log += Logger.Log;

			service.GetRequiredService<TimerService>().Configure();
			service.GetRequiredService<DiscordEventHandlerService>().Configure();
			await service.GetRequiredService<CommandHandlerService>().ConfigureAsync();


			await shardedClient.LoginAsync(TokenType.Bot, config.Discord.BotToken);
			await shardedClient.StartAsync();
			await shardedClient.SetStatusAsync(UserStatus.Online);

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
				.AddSingleton(new DiscordShardedClient(config.Discord.Shards, new DiscordSocketConfig
				{
					AlwaysDownloadUsers = true,
					LogLevel = LogSeverity.Verbose,
					DefaultRetryMode = RetryMode.AlwaysRetry,
					MessageCacheSize = 100,
					TotalShards = config.Discord.Shards.Length
				}))
				.AddDbContext<FailsafeContext>()
				.AddSingleton<CommandService>()
				.AddSingleton<CommandHandlerService>()
				.AddSingleton<DiscordEventHandlerService>()
				.AddSingleton<InteractiveService>()
				.AddSingleton<TimerService>()
				.AddSingleton<MilestoneService>()
				.BuildServiceProvider();
		}
		#endregion


	}
}
