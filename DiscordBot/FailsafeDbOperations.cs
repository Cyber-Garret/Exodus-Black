using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Core.Models.Db;

namespace Core
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
				if (gear == null)
					return null;
				else
					return gear;
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


		internal static Task SaveGuildAccountAsync(ulong GuildId, Guild guildAccount)
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
						Context.SaveChangesAsync();

						return Task.CompletedTask;
					}
					else
					{
						Context.Entry(guildAccount).State = EntityState.Modified;
						Context.SaveChangesAsync();
						return Task.CompletedTask;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return Task.CompletedTask;
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
	}
}
