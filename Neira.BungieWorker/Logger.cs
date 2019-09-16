using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Neira.BungieWorker
{
	internal class Logger
	{
		private static readonly Logger instance = new Logger();
		internal static ILogger Log;

		private Logger()
		{
			//re
			var path = Path.Combine(Directory.GetCurrentDirectory(), "logs", "BungieWorker.txt");

			Log = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.WriteTo.Console()
				.WriteTo.File(path, rollingInterval: RollingInterval.Day)
				.CreateLogger();
		}

		public static Logger GetInstance()
		{
			return instance;
		}

	}
}
