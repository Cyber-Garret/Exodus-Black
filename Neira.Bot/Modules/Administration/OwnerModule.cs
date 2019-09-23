using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Neira.API.Bungie;
using Neira.Db;
using Neira.Db.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Neira.Bot.Modules.Administration
{
	public class OwnerModule : BotModuleBase
	{
		private readonly NeiraContext Db;
		public OwnerModule(NeiraContext neiraContext)
		{
			Db = neiraContext;
		}

		#region Functions
		private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
		private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
		private bool Destiny2ClanExists(long id)
		{
			return Db.Clans.AsNoTracking().Any(c => c.Id == id);
		}
		//private bool ProfileExists(string destinyMembershipId)
		//{
		//	using (FailsafeContext failsafeContext = new FailsafeContext())
		//	{
		//		return failsafeContext.Clan_Members.Any(m => m.DestinyMembershipId == destinyMembershipId);
		//	}
		//}
		#endregion

		[Command("add clan")]
		[RequireOwner(ErrorMessage = "Эта команда доступна только моему создателю.")]
		public async Task AddClan(long ClanId)
		{
			try
			{
				if (Destiny2ClanExists(ClanId))
				{
					await ReplyAndDeleteAsync("Клан с таким ID уже зарегистрирован.");
					return;
				}
				var message = await Context.Channel.SendMessageAsync("Начинаю работать");

				var bungie = new BungieApi();
				var claninfo = bungie.GetGroupResult(ClanId);
				if (claninfo.ErrorCode == 1)
				{
					Clan clan = new Clan
					{

						Id = claninfo.Response.Detail.GroupId,
						Name = claninfo.Response.Detail.Name,
						CreateDate = claninfo.Response.Detail.CreationDate,
						Motto = claninfo.Response.Detail.Motto,
						About = claninfo.Response.Detail.About,
						MemberCount = claninfo.Response.Detail.MemberCount
					};


					Db.Clans.Add(clan);
					await Db.SaveChangesAsync();
					await message.ModifyAsync(m => m.Content = "Готово");
				}
				else
				{
					await message.ModifyAsync(m => m.Content = $"Ошибка регистрации клана {claninfo.ErrorStatus} - {claninfo.Message}");
				}

			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "AddClan", ex.Message, ex));
				Console.WriteLine(ex.ToString());
				await ReplyAsync($"Ошибка добавления клана. Подробности в консоли.\nСообщение: {ex.Message}");
			}

		}

		[Command("stat")]
		[Summary("Выводит техническую информацию о боте.")]
		[RequireOwner(ErrorMessage = "Эта команда доступна только моему создателю.")]
		public async Task InfoAsync()
		{
			var app = await Context.Client.GetApplicationInfoAsync();

			var embed = new EmbedBuilder();
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
				$"- Пользователей: {Context.Client.Guilds.Sum(g => g.Users.Count)}\n" +
				$"- Текущее время сервера: {DateTime.Now}", true);

			await ReplyAsync(null, false, embed.Build());
		}
	}
}
