using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Discord;

using Core;
using Core.Models.Db;

namespace DiscordBot
{
	internal static class FailsafeDbOperations
	{
		#region Exotics

		/// <summary>
		/// Поиск экзотического снаряжения в БД по названию вещи.
		/// </summary>
		/// <param name="ItemName">полное или частичное название вещи для поиска в бд.</param>
		/// <returns>Gear class</returns>
		internal static Gear GetGears(string ItemName)
		{
			using (var Context = new FailsafeContext())
			{
				Gear gear = Context.Gears.Where(g => g.Name.IndexOf(ItemName, StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefault();
				return gear;
			}
		}

		internal static Catalyst GetCatalyst(string name)
		{
			using (var Db = new FailsafeContext())
			{
				if (name.ToLower() == "любой")
				{
					Random r = new Random();
					int randomId = r.Next(1, Db.Catalysts.Count());
					return Db.Catalysts.Skip(randomId).Take(1).FirstOrDefault();
				}
				else
				{
					Catalyst catalyst = Db.Catalysts.Where(c => c.WeaponName.IndexOf(name, StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefault();
					return catalyst;
				}
			}
		}
		#endregion

		#region Guilds

		/// <summary>
		/// Возвращает список всех гильдий.
		/// </summary>
		/// <returns>IEnumerable<Guild></returns>
		internal static async Task<IEnumerable<Guild>> GetAllGuildsAsync()
		{
			using (var Context = new FailsafeContext())
			{
				IEnumerable<Guild> guilds = await Context.Guilds.ToListAsync();
				return guilds;
			}
		}

		/// <summary>
		/// Возвращает данные о гильдии, если нет данных создает запись.
		/// </summary>
		/// <param name="guildId">Discord SocketGuild Id</param>
		/// <returns>Guild class</returns>
		internal static async Task<Guild> GetGuildAccountAsync(ulong guildId)
		{
			using (var Context = new FailsafeContext())
			{
				if (Context.Guilds.Where(G => G.ID == guildId).Count() < 1)
				{
					var newGuild = new Guild
					{
						ID = guildId
					};

					Context.Guilds.Add(newGuild);
					await Context.SaveChangesAsync();

					return newGuild;
				}
				else
				{
					return Context.Guilds.SingleOrDefault(G => G.ID == guildId);
				}
			}
		}


		internal static async Task SaveGuildAccountAsync(ulong GuildId, Guild guildAccount)
		{
			try
			{
				using (var Context = new FailsafeContext())
				{
					if (Context.Guilds.Where(G => G.ID == GuildId).Count() < 1)
					{
						var newGuild = new Guild
						{
							ID = GuildId
						};

						Context.Guilds.Add(newGuild);
						await Context.SaveChangesAsync();
					}
					else
					{
						Context.Entry(guildAccount).State = EntityState.Modified;
						await Context.SaveChangesAsync();
					}
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, Logger.GetExecutingMethodName(ex), ex.Message, ex));
			}

		}

		internal static Task SaveWelcomeMessage(ulong GuildId, string value)
		{

			using (var Context = new FailsafeContext())
			{
				var GuildData = Context.Guilds.First(g => g.ID == GuildId);
				GuildData.WelcomeMessage = value;
				Context.Guilds.Update(GuildData);
				Context.SaveChanges();
				return Task.CompletedTask;
			}
		}
		#endregion

		#region Raids
		internal static async Task<RaidInfo> GetRaidInfo(string raidName)
		{
			using (var Db = new FailsafeContext())
			{
				var raid = await Db.RaidInfos.Where(r =>
				r.Name.IndexOf(raidName, StringComparison.CurrentCultureIgnoreCase) != -1 ||
				r.Alias.IndexOf(raidName, StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefaultAsync();
				return raid;
			}
		}

		internal static async Task<IEnumerable<RaidInfo>> GetAllRaidsInfo()
		{
			using (var Context = new FailsafeContext())
			{
				IEnumerable<RaidInfo> raids = await Context.RaidInfos.ToListAsync();
				return raids;
			}
		}

		internal static async Task<ActiveRaid> GetRaidAsync(ulong msgId)
		{
			using (var Db = new FailsafeContext())
			{
				try
				{

				}
				catch (Exception ex)
				{
					await Logger.Log(new LogMessage(LogSeverity.Error, Logger.GetExecutingMethodName(ex), ex.Message, ex));
					throw;
				}
				var raid = Db.ActiveRaids.Include(r => r.RaidInfo).Where(r => r.MessageId == msgId).FirstOrDefault();
				return raid;
			}
		}

		internal static async Task SaveRaidAsync(ActiveRaid raid)
		{
			using (var Db = new FailsafeContext())
			{
				try
				{
					if (Db.ActiveRaids.Where(r => r.MessageId == raid.MessageId).Count() < 1)
					{
						Db.ActiveRaids.Add(raid);
						await Db.SaveChangesAsync();
					}
					else
					{
						Db.ActiveRaids.Update(raid);
						await Db.SaveChangesAsync();
					}
				}
				catch (Exception ex)
				{
					await Logger.Log(new LogMessage(LogSeverity.Error, Logger.GetExecutingMethodName(ex), ex.Message, ex));
				}
			}
		}
		#endregion
	}
}
