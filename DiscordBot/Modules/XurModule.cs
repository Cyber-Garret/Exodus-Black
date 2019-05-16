using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using DiscordBot.Preconditions;

namespace DiscordBot.Modules.Commands
{
	public class XurModule : BotModuleBase
	{
		#region Global fields
		readonly DateTime today = DateTime.Now;
		static TimeSpan start = new TimeSpan(20, 0, 0);
		static TimeSpan end = start; //setting the same value as your start and end time is same
		DateTime fridayStartCheck = new DateTime();
		DateTime tuesdayEndCheck = new DateTime();
		#endregion

		[Command("зур"), Summary("Информационная комманда о посланнике девяти по имени Зур")]
		[Cooldown(10)]
		public async Task XurCommand()
		{

			#region Check DayOfWeek
			if (today.DayOfWeek == DayOfWeek.Friday)
			{
				fridayStartCheck = DateTime.Parse(today.ToShortDateString());
				fridayStartCheck += start;
				tuesdayEndCheck = DateTime.Parse(today.AddDays(+5).ToShortDateString());
				tuesdayEndCheck += end;
			}
			if (today.DayOfWeek == DayOfWeek.Saturday)
			{
				fridayStartCheck = DateTime.Parse(today.AddDays(-1).ToShortDateString());
				fridayStartCheck += start;
				tuesdayEndCheck = DateTime.Parse(today.AddDays(+4).ToShortDateString());
				tuesdayEndCheck += end;
			}
			if (today.DayOfWeek == DayOfWeek.Sunday)
			{
				fridayStartCheck = DateTime.Parse(today.AddDays(-2).ToShortDateString());
				fridayStartCheck += start;
				tuesdayEndCheck = DateTime.Parse(today.AddDays(+3).ToShortDateString());
				tuesdayEndCheck += end;
			}
			if (today.DayOfWeek == DayOfWeek.Monday)
			{
				fridayStartCheck = DateTime.Parse(today.AddDays(-3).ToShortDateString());
				fridayStartCheck += start;
				tuesdayEndCheck = DateTime.Parse(today.AddDays(+2).ToShortDateString());
				tuesdayEndCheck += end;
			}
			if (today.DayOfWeek == DayOfWeek.Tuesday)
			{
				fridayStartCheck = DateTime.Parse(today.AddDays(-5).ToShortDateString());
				fridayStartCheck += start;
				tuesdayEndCheck = DateTime.Parse(today.ToShortDateString());
				tuesdayEndCheck += end;
			}
			#endregion

			if (today >= fridayStartCheck && today <= tuesdayEndCheck)
			{
				EmbedBuilder Embed = new EmbedBuilder()
					.WithColor(Color.Gold)
					.WithTitle($"Уважаемый пользователь {Context.User}")
					.WithDescription("По моим данным Зур в данный момент в пределах солнечной системы, но\n" +
					"так как мои алгоритмы глобального позиционирования пока еще в разработке, определить точное положение я не могу.\n" +
					"[Но я уверена что тут ты сможешь отыскать его положение](https://whereisxur.com/)\n" +
					"[Или тут](https://ftw.in/game/destiny-2/find-xur)")
					.WithFooter("Напоминаю! Зур покинет пределы солнечной системы во вторник 20:00 по МСК.");
				await Context.Channel.SendMessageAsync("", false, Embed.Build());
			}
			else
			{
				EmbedBuilder Embed = new EmbedBuilder()
					.WithColor(Color.Red)
					.WithDescription("По моим данным Зур не в пределах солнечной системы.")
					.WithFooter("Зур прибудет в пятницу 20:00 по МСК. Я сообщу.");
				await Context.Channel.SendMessageAsync("", false, Embed.Build());
			}
		}
	}
}
