using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

using Failsafe.Core.QuartzJobs;
using Failsafe.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Neiralink;

using Quartz;
using Quartz.Impl;
using Quartz.Spi;

using Serilog;

using System;

namespace Failsafe
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            Log.Information("Wake the fuck up, Samurai. We have a city to burn.");

            try
            {
                CreateHostBuilder(args).Build().Run();
                Log.Information("Stopped clearly.");
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Fatal in main.");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console())
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Neira>();
                    // Quartz services
                    services.AddHostedService<Quartz>();
                    services.AddSingleton<IJobFactory, SingletonJobFactory>();
                    services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
                    // bot services
                    services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                    {
                        ExclusiveBulkDelete = true,
                        AlwaysDownloadUsers = true,
                        LogLevel = LogSeverity.Info,
                        DefaultRetryMode = RetryMode.AlwaysRetry,
                        MessageCacheSize = 300,
                        GatewayIntents = GatewayIntents.Guilds |
                                             GatewayIntents.GuildMembers |
                                             GatewayIntents.GuildBans |
                                             GatewayIntents.GuildEmojis |
                                             GatewayIntents.GuildIntegrations |
                                             GatewayIntents.GuildWebhooks |
                                             GatewayIntents.GuildInvites |
                                             GatewayIntents.GuildMessages |
                                             GatewayIntents.GuildMessageReactions |
                                             GatewayIntents.DirectMessages |
                                             GatewayIntents.DirectMessageReactions
                    }))
                        .AddSingleton(new CommandService(new CommandServiceConfig
                        {
                            CaseSensitiveCommands = false,
                            DefaultRunMode = RunMode.Async,
                            LogLevel = LogSeverity.Info
                        }))
                        .AddSingleton(new DiscordRestClient(new DiscordRestConfig
                        {
                            DefaultRetryMode = RetryMode.AlwaysRetry,
                            LogLevel = LogSeverity.Info,
                            RateLimitPrecision = RateLimitPrecision.Second
                        }))
                        .AddSingleton<InteractiveService>()
                        .AddSingleton<LoggingService>()
                        .AddSingleton<DiscordEventHandlerService>()
                        .AddSingleton<CommandHandlerService>()
                        .AddSingleton<EmoteService>()
                        .AddSingleton<SelfRoleService>()
                        .AddSingleton<MilestoneService>();
                    // Quartz jobs
                    services.AddSingleton<XurArrive>();
                    services.AddSingleton<XurLeave>();
                    services.AddSingleton<MilestoneRemind>();
                    services.AddSingleton<MilestoneClean>();
                    services.AddSingleton<SaveCommandStatistic>();
                    // Quartz triggers
                    var hour = hostContext.Configuration.GetSection("Bot:XurHour").Get<int>();
                    services.AddSingleton(new JobSchedule(typeof(XurArrive), $"0 0 {hour} ? * FRI")); // run every Friday in 20:00
                    services.AddSingleton(new JobSchedule(typeof(XurLeave), $"0 0 {hour} ? * TUE")); // run every Tuesday in 20:00
                    services.AddSingleton(new JobSchedule(typeof(MilestoneRemind), "0/10 * * * * ?")); // run every 10 seconds.
                    services.AddSingleton(new JobSchedule(typeof(MilestoneClean), "0 0/15 * * * ?")); // run every 15 minute.
                    services.AddSingleton(new JobSchedule(typeof(SaveCommandStatistic), "0 0 * ? * *")); //run every hour.

                    services.AddTransient<IWelcomeDbClient, WelcomeDbClient>(provider => new WelcomeDbClient(hostContext.Configuration.GetConnectionString("DefaultConnection")));
                    services.AddTransient<IWishDbClient, WishDbClient>(provider => new WishDbClient(hostContext.Configuration.GetConnectionString("DefaultConnection")));
                });
    }
}
