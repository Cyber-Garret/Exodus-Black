using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Discord;

namespace Bot
{
	internal class Logger
	{
		/// <summary>
		/// LogSeverity colors in console - Critical = Red; Error = Yellow; Warning = Magenta; Info = Cyan; Verbose = Green; Debug = Blue;
		/// </summary>
		/// <param name="logMessage"></param>
		/// <returns></returns>
		internal static Task Log(LogMessage logMessage)
		{
			Console.ForegroundColor = SeverityToConsoleColor(logMessage.Severity);
			string message = $"[{DateTime.Now.ToLongTimeString()}, Source: {logMessage.Source}] {logMessage.Message}";
			if (logMessage.Exception != null)
				message += $"\n{logMessage.Exception}\n";

			Console.WriteLine(message);
			Console.ResetColor();
			return Task.CompletedTask;
		}

		/// <summary>
		/// Change console color by log severity value
		/// </summary>
		/// <param name="severity">Log severity type</param>
		/// <returns>Console color</returns>
		private static ConsoleColor SeverityToConsoleColor(LogSeverity severity)
		{
			switch (severity)
			{
				case LogSeverity.Critical:
					return ConsoleColor.Red;
				case LogSeverity.Error:
					return ConsoleColor.Yellow;
				case LogSeverity.Warning:
					return ConsoleColor.Magenta;
				case LogSeverity.Info:
					return ConsoleColor.Cyan;
				case LogSeverity.Verbose:
					return ConsoleColor.Green;
				case LogSeverity.Debug:
					return ConsoleColor.Blue;
				default:
					return ConsoleColor.White;
			}
		}
	}
}
