using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;

namespace Neira.Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.CreateLogger();
			try
			{
				Log.Information("Starting up");
				CreateHostBuilder(args).Build().Run();
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Application start-up failed");
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
			.UseSerilog()
			.ConfigureLogging((context, logging) =>
			{
				var env = context.HostingEnvironment;
				var config = context.Configuration.GetSection("Logging");

				logging.AddConfiguration(config);
				logging.AddConsole();
				logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
			})
			.ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder.UseStartup<Startup>();
			});
	}
}
