using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Neuromatrix.Resources;
using Neuromatrix.Services;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nett;

namespace Neuromatrix
{
    public class Program : IDisposable
    {
        public static void Main(string[] args)
        {
            using (var program = new Program())
                program.MainAsync().GetAwaiter().GetResult();
        }

        private readonly CancellationTokenSource _exitTokenSource;
        private readonly ServiceProvider _services;

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
            _services.GetRequiredService<LogService>().Configure();
            await _services.GetRequiredService<CommandHandlingService>().ConfigureAsync();

            var token = _services.GetRequiredService<Settings>().Token;
            var discord = _services.GetRequiredService<DiscordSocketClient>();

            await discord.LoginAsync(TokenType.Bot, token);
            await discord.StartAsync();

            Console.Title = $"Neuromatrix (Discord.Net v{DiscordConfig.Version})";

            try
            {
                await Task.Delay(-1, _exitTokenSource.Token);
                await discord.SetGameAsync("Destiny 2", null, ActivityType.Watching);
            }
            // we expect this to throw when exiting.
            catch (TaskCanceledException) { }

            await discord.StopAsync();
            Environment.Exit(0);
        }

        public ServiceProvider BuildServices()
        {
            return new ServiceCollection()
                .AddSingleton(_ => Toml.ReadFile<Settings>("./Data/config.tml"))
                .AddSingleton(_exitTokenSource)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<LogService>()
                .AddSingleton<RateLimitService>()
                .BuildServiceProvider();
        }

        public void Dispose()
        {
            _exitTokenSource.Dispose();
            _services.Dispose();
        }

    }
}
