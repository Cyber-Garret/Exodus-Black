using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Discord;
using Discord.Commands;

using Core;
using Core.Models.Destiny2;

using API.Bungie;
using API.Bungie.Models;
using Discord.WebSocket;

namespace DiscordBot.Modules.Administration
{
	public class OwnerModule : BotModuleBase
	{
		#region Functions
		private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
		private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
		private bool Destiny2ClanExists(long id)
		{
			using (FailsafeContext context = new FailsafeContext())
			{
				return context.Clans.Any(c => c.Id == id);
			}
		}
		private bool ProfileExists(string destinyMembershipId)
		{
			using (FailsafeContext failsafeContext = new FailsafeContext())
			{
				return failsafeContext.Clan_Members.Any(m => m.DestinyMembershipId == destinyMembershipId);
			}
		}
		#endregion

		[Command("add clan")]
		[RequireOwner(ErrorMessage = "Эта команда доступна только моему создателю.")]
		public async Task AddClan(long ClanId)
		{
			try
			{
				if (Destiny2ClanExists(ClanId))
				{
					await Context.Channel.SendMessageAsync("Клан с таким ID уже зарегистрирован.");
					return;
				}
				var message = await Context.Channel.SendMessageAsync("Начинаю работать");
				using (FailsafeContext failsafe = new FailsafeContext())
				{
					BungieApi bungie = new BungieApi();
					var claninfo = bungie.GetGroupResult(ClanId);
					if (claninfo.Response != null)
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


						failsafe.Add(clan);
						await failsafe.SaveChangesAsync();
					}
					await message.ModifyAsync(m => m.Content = "Готово");
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, Logger.GetExecutingMethodName(ex), ex.Message, ex));
				Console.WriteLine(ex.ToString());
				await Context.Channel.SendMessageAsync("Ошибка добавления клана. Подробности в консоли.");
			}

		}


		[Command("reload")]
		[RequireOwner(ErrorMessage = "Эта команда доступна только моему создателю.")]
		public async Task ReloadMembers()
		{
			try
			{

				var message = await Context.Channel.SendMessageAsync("Начинаю работать");
				using (FailsafeContext failsafeContext = new FailsafeContext())
				{
					var members = failsafeContext.Clan_Members.ToList();
					BungieApi bungieApi = new BungieApi();
					foreach (var item in members)
					{
						var profile = bungieApi.GetProfileResult(item.DestinyMembershipId, BungieMembershipType.TigerBlizzard, DestinyComponentType.Profiles);

						var member = failsafeContext.Clan_Members.Single(m => m.DestinyMembershipId == item.DestinyMembershipId);

						member.DateLastPlayed = profile.Response.Profile.Data.DateLastPlayed;

						failsafeContext.Update(member);
						await failsafeContext.SaveChangesAsync();
					}
				}
				await message.ModifyAsync(m => m.Content = "Готово");
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, Logger.GetExecutingMethodName(ex), ex.Message, ex));
				Console.WriteLine(ex.ToString());
			}

		}

		[Command("stat")]
		[Summary("Выводит техническую информацию о боте.")]
		[RequireOwner(ErrorMessage = "Эта команда доступна только моему создателю.")]
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
				$"- Пользователей: {Context.Client.Guilds.Sum(g => g.Users.Count)}\n" +
				$"- Текущее время сервера: {DateTime.Now}", true);

			await Context.Channel.SendMessageAsync(null, false, embed.Build());
		}

		[Command("clean")]
		[Summary("Удаляет заданное количество сообщений где была вызвана команда.")]
		[RequireOwner]
		[RequireBotPermission(ChannelPermission.ManageMessages)]
		public async Task PurgeChat(int amount)
		{
			try
			{
				await ReplyAsync("Начинаю очистку канала.");
				var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
				if (messages.Count() < 1)
					return;
				foreach (var item in messages)
				{
					await Task.Delay(500);
					await (Context.Channel as ITextChannel).DeleteMessageAsync(item.Id);
				}
				//await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages);

				const int delay = 5000;
				var m = await Context.Channel.SendMessageAsync($"Задание успешно выполнено. _Это сообщение будет удалено через {delay / 1000} сек._");
				await Task.Delay(delay);
				await m.DeleteAsync();
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, Logger.GetExecutingMethodName(ex), ex.Message, ex));
				Console.WriteLine(ex.ToString());
				await Context.Channel.SendMessageAsync($"Ошибка очистки канала от {amount} сообщений. {ex.Message}.");
			}
		}
	}
}
