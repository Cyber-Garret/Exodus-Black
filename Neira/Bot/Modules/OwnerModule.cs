using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Neira.API.Bungie;
using Neira.Database;
using Neira.Bot.Helpers;
using Neira.Bot.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Neira.Bot.Modules
{
	public class OwnerModule : BaseModule
	{
		private readonly ILogger _logger;
		public OwnerModule(IServiceProvider service)
		{
			_logger = service.GetRequiredService<ILogger<OwnerModule>>();
		}
		#region Functions
		private string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
		private string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
		private bool Destiny2ClanExists(long id)
		{
			using var Db = new NeiraLinkContext();
			return Db.Clans.AsNoTracking().Any(c => c.Id == id);
		}
		#endregion

		[Command("addclan")]
		public async Task AddClan(long ClanId)
		{
			if (Context.User.Id != GlobalVariables.Cyber_Garret) return;
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
					using var Db = new NeiraLinkContext();

					var clan = new Clan
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
				await ReplyAsync($"Ошибка добавления клана. Подробности в консоли.\nСообщение: {ex.Message}");
				_logger.LogWarning(ex, "AddClan");
			}

		}

		[Command("syncclan")]
		public async Task AssociateClan(ulong DiscordGuildId, long DestinyClanId)
		{
			if (Context.User.Id != GlobalVariables.Cyber_Garret) return;
			try
			{
				using var Db = new NeiraLinkContext();
				//Find Destiny 2 clan by ID
				var clan = Db.Clans.FirstOrDefault(c => c.Id == DestinyClanId);
				//If not found reply
				if (clan == null)
					await ReplyAndDeleteAsync($"Destiny clan with ID **{DestinyClanId}** not found in Database");
				else
				{
					//Get discord guild by ID
					var guild = Context.Client.GetGuild(DiscordGuildId);

					//Check if Destiny 2 clan already associanet to discord guild
					if (clan.GuildId != null)
						await ReplyAndDeleteAsync($"Destiny clan **{clan.Name}** already associated with Discord guild **{guild.Name}**.");
					else
					{
						//Store discord guild id in database
						clan.GuildId = DiscordGuildId;
						Db.Clans.Update(clan);
						Db.SaveChanges();

						await ReplyAsync($"Destiny clan **{clan.Name}** success associated with Discord guild **{guild.Name}**.");
					}
				}
			}
			catch (Exception ex)
			{
				await ReplyAndDeleteAsync(ex.Message);
				_logger.LogWarning(ex, "SyncClan");
			}
		}

		[Command("stat")]
		[Summary("Выводит техническую информацию о боте.")]
		public async Task InfoAsync()
		{
			if (Context.User.Id != GlobalVariables.Cyber_Garret) return;
			var embed = new EmbedBuilder();
			embed.WithColor(Color.Green);
			embed.WithTitle("Моя техническая информация");
			embed.AddField("Статистика",
				$"- Heap Size: {GetHeapSize()}MiB\n" +
				$"- Всего серверов: {Context.Client.Guilds.Count}\n" +
				$"- Всего каналов: {Context.Client.Guilds.Sum(g => g.Channels.Count)}\n" +
				$"- Пользователей: {Context.Client.Guilds.Sum(g => g.Users.Count)}\n" +
				$"- Текущее время сервера: {DateTime.Now}\n" +
				$" - UpTime: { GetUptime()}", true);

			await ReplyAsync(embed: embed.Build());
		}

		[Command("searchguild"), Alias("sg")]
		public async Task SearchGuild([Remainder]string name)
		{
			if (Context.User.Id != GlobalVariables.Cyber_Garret) return;
			if (string.IsNullOrWhiteSpace(name))
				await ReplyAndDeleteAsync("Название Discord сервера не было представлено");
			else
			{
				var guild = Context.Client.Guilds.FirstOrDefault(g => g.Name.IndexOf(name, StringComparison.InvariantCultureIgnoreCase) != -1);
				if (guild == null)
					await ReplyAndDeleteAsync($"Discord сервер с названием **{name}** не найден.");
				else
					await ReplyAsync($"Найден сервер **{guild.Name}** с ID **{guild.Id}**");
			}
		}

		[Command("ServerInfo")]
		public async Task GuildInfo(ulong GuildId)
		{
			if (Context.User.Id != GlobalVariables.Cyber_Garret) return;
			try
			{
				var guild = Context.Client.Guilds.FirstOrDefault(g => g.Id == GuildId);
				if (guild == null)
				{
					await ReplyAsync($"Неудалось найти гильдию с ID **{GuildId}**");
					return;
				}
				else
				{
					var embed = new EmbedBuilder
					{
						Title = $"Капитан, статистика сервера `{guild.Name}`",
						Color = Color.Gold,
						Description =
						$"ID: {guild.Id}\n" +
						$"Владелец: {guild.Owner.Username} ID {guild.OwnerId}\n" +
						$"Стражей: {guild.Users.Count}\n" +
						$"Каналов: {guild.Channels.Count}"
					};
					await ReplyAsync(embed: embed.Build());
				}
			}
			catch (Exception ex)
			{
				await ReplyAsync($"Капитан, произошла критическая ошибка: **{ex.Message}**");
				_logger.LogWarning(ex, "ServerInfoCommand");
			}

		}

		[Command("LeaveServer")]
		public async Task LeaveServer(ulong GuildId)
		{
			if (Context.User.Id != GlobalVariables.Cyber_Garret) return;
			try
			{
				var guild = Context.Client.Guilds.FirstOrDefault(g => g.Id == GuildId);
				if (guild == null)
				{
					await ReplyAsync($"Неудалось найти гильдию с ID **{GuildId}**");
					return;
				}
				else
				{
					await guild.LeaveAsync();
					await ReplyAsync($"Капитан, я покинула гильдию **{guild.Name}**");
				}
			}
			catch (Exception ex)
			{
				await ReplyAsync($"Капитан, произошла критическая ошибка: **{ex.Message}**");
				_logger.LogWarning(ex, "LeaveGuildCommand");
			}
		}
	}
}
