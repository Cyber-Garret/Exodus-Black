using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Discord;

namespace DiscordBot
{
	internal class Logger
	{
		internal static Task Log(LogMessage logMessage)
		{
			Console.ForegroundColor = SeverityToConsoleColor(logMessage.Severity);
			string message = string.Concat("[", DateTime.Now.ToLongTimeString(), " Source: ", logMessage.Source, "] ", logMessage.Message);
			Console.WriteLine(message);
			Console.ResetColor();
			return Task.CompletedTask;
		}

		internal static string GetExecutingMethodName(Exception exception)
		{
			var trace = new StackTrace(exception);
			var frame = trace.GetFrame(0);
			var method = frame.GetMethod();

			return string.Concat(method.DeclaringType.FullName, ".", method.Name);
		}

		private static ConsoleColor SeverityToConsoleColor(LogSeverity severity)
		{
			switch (severity)
			{
				case LogSeverity.Critical:
					return ConsoleColor.Red;
				case LogSeverity.Debug:
					return ConsoleColor.Blue;
				case LogSeverity.Error:
					return ConsoleColor.Yellow;
				case LogSeverity.Info:
					return ConsoleColor.Blue;
				case LogSeverity.Verbose:
					return ConsoleColor.Green;
				case LogSeverity.Warning:
					return ConsoleColor.Magenta;
				default:
					return ConsoleColor.White;
			}
		}
	}
}
