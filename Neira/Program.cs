using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neira.Models;
using Serilog;
using Serilog.Events;
using System;

namespace Neira
{
	public class Program
	{
		private static IConfiguration _config { get; set; }

		public static void Main(string[] args)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("UserData/config.json");

			_config = builder.Build();

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
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
			.ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder.UseUrls("http://localhost:5050");
				webBuilder.UseStartup<Startup>();
			})
			.ConfigureServices(service =>
			{
				service.Configure<Config>(_config);
			});
	}
}
