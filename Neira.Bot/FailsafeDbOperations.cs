using Discord;
using Microsoft.EntityFrameworkCore;
using Neira.Db;
using Neira.Db.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Bot
{
	internal static class FailsafeDbOperations
	{

		#region Guilds

		/// <summary>
		/// Возвращает список всех гильдий.
		/// </summary>
		/// <returns>IEnumerable<Guild></returns>
		internal static async Task<IEnumerable<Guild>> GetAllGuildsAsync()
		{
			try
			{
				using (var Context = new NeiraContext())
				{
					IEnumerable<Guild> guilds = await Context.Guilds.ToListAsync();
					return guilds;
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Critical, "GetAllGuilds", ex.Message, ex));
				return null;
			}
		}

		/// <summary>
		/// Возвращает данные о гильдии, если нет данных создает запись.
		/// </summary>
		/// <param name="guildId">Discord SocketGuild Id</param>
		/// <returns>Guild class</returns>
		internal static async Task<Guild> GetGuildAccountAsync(ulong guildId)
		{
			try
			{
				using (var Context = new NeiraContext())
				{
					if (Context.Guilds.Where(G => G.Id == guildId).Count() < 1)
					{
						var newGuild = new Guild
						{
							Id = guildId
						};

						Context.Guilds.Add(newGuild);
						await Context.SaveChangesAsync();

						return newGuild;
					}
					else
					{
						return Context.Guilds.SingleOrDefault(G => G.Id == guildId);
					}
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Critical, "GetGuildAccount", ex.Message, ex));
				return null;
			}
		}


		internal static async Task SaveGuildAccountAsync(ulong GuildId, Guild guildAccount)
		{
			try
			{
				using (var Context = new NeiraContext())
				{
					if (Context.Guilds.Where(G => G.Id == GuildId).Count() < 1)
					{
						var newGuild = new Guild
						{
							Id = GuildId
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
				await Logger.Log(new LogMessage(LogSeverity.Error, "SaveGuildAccount", ex.Message, ex));
			}

		}

		internal static async void SaveWelcomeMessage(ulong GuildId, string value)
		{
			try
			{
				using (var Context = new NeiraContext())
				{
					var GuildData = Context.Guilds.First(g => g.Id == GuildId);
					GuildData.WelcomeMessage = value;
					Context.Guilds.Update(GuildData);
					Context.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Critical, "SaveWelcomeMessage", ex.Message, ex));
			}

		}
		#endregion

		#region Milestones
		internal static async Task<Milestone> GetMilestone(string milestoneName)
		{
			using (var Db = new NeiraContext())
			{
				var milestone = await Db.Milestones.Where(r =>
				r.Name.IndexOf(milestoneName, StringComparison.CurrentCultureIgnoreCase) != -1 ||
				r.Alias.IndexOf(milestoneName, StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefaultAsync();
				return milestone;
			}
		}

		internal static async Task<IEnumerable<Milestone>> GetAllMilestones()
		{
			using (var Context = new NeiraContext())
			{
				IEnumerable<Milestone> milestones = await Context.Milestones.ToListAsync();
				return milestones;
			}
		}

		internal static async Task<ActiveMilestone> GetActiveMilestone(ulong msgId)
		{
			using (var Db = new NeiraContext())
			{
				ActiveMilestone activeMilestone = await Db.ActiveMilestones.Include(r => r.Milestone).Where(r => r.MessageId == msgId).FirstOrDefaultAsync();
				return activeMilestone;
			}
		}

		internal static async Task SaveActiveMilestone(ActiveMilestone activeMilestone)
		{
			using (var Db = new NeiraContext())
			{
				try
				{
					if (Db.ActiveMilestones.Where(r => r.MessageId == activeMilestone.MessageId).Count() < 1)
					{
						Db.ActiveMilestones.Add(activeMilestone);
						await Db.SaveChangesAsync();
					}
					else
					{
						Db.ActiveMilestones.Update(activeMilestone);
						await Db.SaveChangesAsync();
					}
				}
				catch (Exception ex)
				{
					await Logger.Log(new LogMessage(LogSeverity.Error, "SaveActiveMilestone", ex.Message, ex));
				}
			}
		}
		#endregion
	}
}
