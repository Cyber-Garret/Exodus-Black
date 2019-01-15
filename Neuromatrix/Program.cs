using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using Nett;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Neuromatrix.Models;
using Neuromatrix.Services;

namespace Neuromatrix
{
    public class Program : IDisposable
    {

        public static void Main(string[] args)
        {
            using (var program = new Program())
                program.MainAsync().GetAwaiter().GetResult();
        }

        #region Private fields
        private readonly CancellationTokenSource _exitTokenSource;
        private readonly ServiceProvider _services;
        #endregion

        public Program()
        {
            _exitTokenSource = new CancellationTokenSource();
            _services = BuildServices();

            Console.CancelKeyPress += (_s, e) =>
            {
                e.Cancel = true;
                _exitTokenSource.Cancel();
            };
        }

        private async Task MainAsync()
        {
            Console.Title = $"Neuromatrix (Discord.Net v{DiscordConfig.Version})";

            #region Configure services
            _services.GetRequiredService<ConfigurationService>().Configure();
            _services.GetRequiredService<LogService>().Configure();
            _services.GetRequiredService<ReminderService>().Configure();
            await _services.GetRequiredService<CommandHandlingService>().ConfigureAsync();
            #endregion

            var token = _services.GetRequiredService<Configuration>().Token;
            var discord = _services.GetRequiredService<DiscordSocketClient>();

            await discord.LoginAsync(TokenType.Bot, token);
            await discord.StartAsync();

            try
            {
                await discord.SetGameAsync("Destiny 2", null, ActivityType.Watching);
                await Task.Delay(-1, _exitTokenSource.Token);
            }
            // we expect this to throw when exiting.
            catch (TaskCanceledException) { }

            await discord.StopAsync();
            Environment.Exit(0);
        }

        public ServiceProvider BuildServices()
        {
            return new ServiceCollection()
                .AddSingleton(_ => Toml.ReadFile<Configuration>("./UserData/config.tml"))
                .AddSingleton(_exitTokenSource)
                .AddSingleton<ConfigurationService>()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<LogService>()
                .AddSingleton<RateLimitService>()
                .AddSingleton<ReminderService>()
                .BuildServiceProvider();
        }

        public void Dispose()
        {
            _exitTokenSource.Dispose();
            _services.Dispose();
        }

    }
}
