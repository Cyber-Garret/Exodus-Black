using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Discord;
using Discord.Commands;
using DiscordBot.Preconditions;

namespace DiscordBot.Modules.Commands
{
	public class HelpModule : BotModuleBase
	{
		#region Functions
		private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
		private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
		#endregion

		[Command("справка")]
		[Summary("Основная справочная команда.")]
		[Cooldown(10)]
		public async Task MainHelp()
		{
			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.WithColor(Color.Gold);
			embedBuilder.WithDescription(
				$"Доброго времени суток. Я Нейроматрица версии {Global.Version}.\n" +
				"Моя основная цель своевременно сообщать когда прибывает или улетает посланник девяти Зур.\n" +
				"Так же я могу предоставить информацию о экзотическом вооружении.\n" +
				"В текущий момент в моей базе данных зарегистрированны такие команды:");
			embedBuilder.AddField(
				"Команда: **!инфо [название снаряжения]**",
				"Эта команда несет полную информацию о экзотическом вооружении представленном в игре.\n" +
				"Синтаксис довольно простой, можно искать как по полному названию так и частичному.\n" +
				"Например: **!инфо дело** предоставит информацию об автомате Милое Дело.");
			embedBuilder.AddField(
				"Команда: **!зур**",
				"Команда отображает находится ли в данный момент Зур в игре или нет.");
			embedBuilder.WithFooter("Для дальнейшей работы введите одну из команд представленых выше.");

			await Context.Channel.SendMessageAsync("", false, embedBuilder.Build());
		}

		[Command("статистика")]
		[Summary("Выводит техническую информацию о боте.")]
		[RequireOwner()]
		public async Task InfoAsync()
		{
			var app = await Context.Client.GetApplicationInfoAsync();

			EmbedBuilder embed = new EmbedBuilder();
			embed.WithColor(Color.Green);
			embed.WithTitle("Моя техническая информация");
			embed.AddField("Инфо",
				$"- Автор: {app.Owner}\n" +
				$"- Библиотека: Discord.Net ({DiscordConfig.Version})\n" +
				$"- Среда выполнения: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture} " +
					$"({RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture})\n" +
				$"- UpTime: {GetUptime()}", true);
			embed.AddField("Статистика",
				$"- Heap Size: {GetHeapSize()}MiB\n" +
				$"- Всего серверов: {Context.Client.Guilds.Count}\n" +
				$"- Всего каналов: {Context.Client.Guilds.Sum(g => g.Channels.Count)}\n" +
				$"- Пользователей: {Context.Client.Guilds.Sum(g => g.Users.Count)}", true);

			await Context.Channel.SendMessageAsync(null, false, embed.Build());
		}
	}
}
