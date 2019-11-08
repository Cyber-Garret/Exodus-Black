using Serilog;

namespace BungieWorker
{
	internal class Logger
	{
		private static readonly Logger instance = new Logger();
		internal static ILogger Log;

		private Logger()
		{
			Log = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.WriteTo.Console()
				.CreateLogger();
		}

		public static Logger GetInstance()
		{
			return instance;
		}

	}
}
