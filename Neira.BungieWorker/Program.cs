using System.Threading.Tasks;
using System;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Neira.BungieWorker
{
	class Program
	{

		static void Main()
		{
			Console.Title = $"Neira Bungie Worker: {Assembly.GetEntryAssembly().GetName().Version}";
			Logger.Log.Information("Start the Bungie Worker");
			try
			{
				new Program().StartAsync().GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				Logger.Log.Error(ex, $"[Bungie Worker Main] {ex.Message}");
			}
		}

		private async Task StartAsync()
		{
			_ = new Workers();

			await Task.Delay(-1);
		}
	}
}
