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
	internal class FailsafeDbOperations
	{
		readonly FailsafeContext db;
		public FailsafeDbOperations(FailsafeContext context)
		{
			db = context;
		}
		#region Exotics

		/// <summary>
		/// Поиск экзотического снаряжения в БД по названию вещи.
		/// </summary>
		/// <param name="ItemName">полное или частичное название вещи для поиска в бд.</param>
		/// <returns>Gear class</returns>
		internal Gear GetGears(string ItemName)
		{
			using (db)
			{
				Gear gear = db.Gears.Where(g => g.Name.IndexOf(ItemName, StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefault();
				return gear;
			}
		}

		internal Catalyst GetCatalyst(string name)
		{
			using (db)
			{
				if (name.ToLower() == "любой")
				{
					Random r = new Random();
					int randomId = r.Next(1, db.Catalysts.Count());
					return db.Catalysts.Skip(randomId).Take(1).FirstOrDefault();
				}
				else
				{
					Catalyst catalyst = db.Catalysts.Where(c => c.WeaponName.IndexOf(name, StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefault();
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
		internal async Task<IEnumerable<Guild>> GetAllGuildsAsync()
		{
			using (db)
			{
				IEnumerable<Guild> guilds = await db.Guilds.ToListAsync();
				return guilds;
			}
		}

		/// <summary>
		/// Возвращает данные о гильдии, если нет данных создает запись.
		/// </summary>
		/// <param name="guildId">Discord SocketGuild Id</param>
		/// <returns>Guild class</returns>
		internal async Task<Guild> GetGuildAccountAsync(ulong guildId)
		{
			using (db)
			{
				if (db.Guilds.Where(G => G.ID == guildId).Count() < 1)
				{
					var newGuild = new Guild
					{
						ID = guildId
					};

					db.Guilds.Add(newGuild);
					await db.SaveChangesAsync();

					return newGuild;
				}
				else
				{
					return db.Guilds.SingleOrDefault(G => G.ID == guildId);
				}
			}
		}


		internal async Task SaveGuildAccountAsync(ulong GuildId, Guild guildAccount)
		{
			try
			{
				using (db)
				{
					if (db.Guilds.Where(G => G.ID == GuildId).Count() < 1)
					{
						var newGuild = new Guild
						{
							ID = GuildId
						};

						db.Guilds.Add(newGuild);
						await db.SaveChangesAsync();
					}
					else
					{
						db.Entry(guildAccount).State = EntityState.Modified;
						await db.SaveChangesAsync();
					}
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, Logger.GetExecutingMethodName(ex), ex.Message, ex));
			}

		}

		internal Task SaveWelcomeMessage(ulong GuildId, string value)
		{

			using (db)
			{
				var GuildData = db.Guilds.First(g => g.ID == GuildId);
				GuildData.WelcomeMessage = value;
				db.Guilds.Update(GuildData);
				db.SaveChanges();
				return Task.CompletedTask;
			}
		}
		#endregion

		#region Raids
		internal async Task<RaidInfo> GetRaidInfo(string raidName)
		{
			using (db)
			{
				var raid = await db.RaidInfos.Where(r =>
				r.Name.IndexOf(raidName, StringComparison.CurrentCultureIgnoreCase) != -1 ||
				r.Alias.IndexOf(raidName, StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefaultAsync();
				return raid;
			}
		}

		internal async Task<IEnumerable<RaidInfo>> GetAllRaidsInfo()
		{
			using (db)
			{
				IEnumerable<RaidInfo> raids = await db.RaidInfos.ToListAsync();
				return raids;
			}
		}

		internal async Task<ActiveRaid> GetRaidAsync(ulong msgId)
		{
			using (db)
			{
				try
				{

				}
				catch (Exception ex)
				{
					await Logger.Log(new LogMessage(LogSeverity.Error, Logger.GetExecutingMethodName(ex), ex.Message, ex));
					throw;
				}
				var raid = db.ActiveRaids.Include(r => r.RaidInfo).Where(r => r.MessageId == msgId).FirstOrDefault();
				return raid;
			}
		}

		internal async Task SaveRaidAsync(ActiveRaid raid)
		{
			using (db)
			{
				try
				{
					if (db.ActiveRaids.Where(r => r.MessageId == raid.MessageId).Count() < 1)
					{
						db.ActiveRaids.Add(raid);
						await db.SaveChangesAsync();
					}
					else
					{
						db.ActiveRaids.Update(raid);
						await db.SaveChangesAsync();
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
