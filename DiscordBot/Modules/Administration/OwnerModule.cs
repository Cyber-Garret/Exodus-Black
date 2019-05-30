using API.Bungie;
using API.Bungie.Models;
using Core.Models.Db;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
				return context.Destiny2Clans.Any(c => c.Id == id);
			}
		}
		private bool ProfileExists(string destinyMembershipId)
		{
			using (FailsafeContext failsafeContext = new FailsafeContext())
			{
				return failsafeContext.Destiny2Clan_Members.Any(m => m.DestinyMembershipId == destinyMembershipId);
			}
		}
		#endregion

		[Command("initialize")]
		[RequireOwner]
		public async Task InitializeMembers(long ClanId)
		{
			try
			{
				if (!Destiny2ClanExists(ClanId))
				{
					await Context.Channel.SendMessageAsync("Такая гильдия незарегистрированна.");
					return;
				}
				var message = await Context.Channel.SendMessageAsync("Начинаю работать");
				BungieApi api = new BungieApi();
				var Members = api.GetMembersOfGroupResponse(ClanId).Response;
				using (FailsafeContext failsafeContext = new FailsafeContext())
				{
					foreach (var item in Members.Results)
					{
						var Member = new Destiny2Clan_Member
						{
							DestinyMembershipType = item.DestinyUserInfo.MembershipType,
							DestinyMembershipId = item.DestinyUserInfo.MembershipId,
							ClanJoinDate = item.JoinDate,
							Destiny2ClanId = item.GroupId
						};
						if (item.BungieNetUserInfo != null)
						{
							Member.Name = item.BungieNetUserInfo.DisplayName;
							Member.BungieMembershipType = item.BungieNetUserInfo.MembershipType;
							Member.BungieMembershipId = item.BungieNetUserInfo.MembershipId;
							Member.IconPath = item.BungieNetUserInfo.IconPath;
						}
						else
						{
							Member.Name = item.DestinyUserInfo.DisplayName;
						}
						failsafeContext.Add(Member);
						await failsafeContext.SaveChangesAsync();
					}
				}
				await message.ModifyAsync(m => m.Content = "Готово");
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, $"initialize Command - {ex.Source}", ex.Message, ex.InnerException));
				Console.WriteLine(ex.ToString());
			}

		}


		[Command("reload")]
		[RequireOwner]
		public async Task ReloadMembers()
		{
			try
			{

				var message = await Context.Channel.SendMessageAsync("Начинаю работать");
				using (FailsafeContext failsafeContext = new FailsafeContext())
				{
					var members = failsafeContext.Destiny2Clan_Members.ToList();
					BungieApi bungieApi = new BungieApi();
					foreach (var item in members)
					{
						var profile = bungieApi.GetProfileResult(item.DestinyMembershipId, BungieMembershipType.TigerBlizzard, DestinyComponentType.Profiles);

						var member = failsafeContext.Destiny2Clan_Members.Single(m => m.DestinyMembershipId == item.DestinyMembershipId);

						member.DateLastPlayed = profile.Response.Profile.Data.DateLastPlayed;

						failsafeContext.Update(member);
						await failsafeContext.SaveChangesAsync();
					}
				}
				await message.ModifyAsync(m => m.Content = "Готово");
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, $"reload Command - {ex.Source}", ex.Message, ex.InnerException));
				Console.WriteLine(ex.ToString());
			}

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
