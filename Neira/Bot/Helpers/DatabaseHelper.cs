using Discord;

using Microsoft.EntityFrameworkCore;

using Neira.Database;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Bot.Helpers
{
	public static class DatabaseHelper
	{
		#region Discord guilds
		/// <summary>
		/// Find or create Discord guild account
		/// </summary>
		/// <param name="guildId">Discord Guild ID</param>
		/// <returns>Guild model</returns>
		public static async Task<Guild> GetGuildAccountAsync(ulong guildId)
		{
			using var Db = new NeiraLinkContext();
			if (Db.Guilds.Any(g => g.Id == guildId))
				return await Db.Guilds.SingleAsync(G => G.Id == guildId);
			else
			{
				var newGuild = new Guild
				{
					Id = guildId
				};

				Db.Guilds.Add(newGuild);
				await Db.SaveChangesAsync();

				return newGuild;
			}
		}

		public static async Task SaveGuildAccountAsync(Guild guildAccount)
		{
			using var Db = new NeiraLinkContext();
			if (Db.Guilds.Any(G => G.Id == guildAccount.Id))
				Db.Guilds.Update(guildAccount);
			else
			{
				var newGuild = new Guild
				{
					Id = guildAccount.Id
				};

				Db.Guilds.Add(newGuild);

			}
			await Db.SaveChangesAsync();
		}

		public static async Task RemoveGuildAccountAsync(ulong guildId)
		{
			using var Db = new NeiraLinkContext();
			if (Db.Guilds.Any(g => g.Id == guildId))
			{
				var guild = Db.Guilds.First(g => g.Id == guildId);
				Db.Guilds.Remove(guild);
				await Db.SaveChangesAsync();
			}
		}

		/// <summary>
		/// Return one role saved as self role.
		/// </summary>
		/// <param name="guildId">Discord guild id</param>
		/// <param name="roleId">Guild role id</param>
		/// <returns>true or false</returns>
		public static bool GetGuildSelfRole(ulong guildId, ulong roleId, ulong emoteId)
		{
			using var Db = new NeiraLinkContext();
			return Db.GuildSelfRoles.Any(g => g.GuildID == guildId && (g.RoleID == roleId || g.EmoteID == emoteId));
		}

		/// <summary>
		/// Get all saved self roles with emote for guild
		/// </summary>
		/// <param name="guildId">Discord guild id</param>
		/// <returns>List of GuildSelfRole</returns>
		public static async Task<List<GuildSelfRole>> GetGuildAllSelfRolesAsync(ulong guildId)
		{
			using var Db = new NeiraLinkContext();
			var roles = await Db.GuildSelfRoles.Where(g => g.GuildID == guildId).ToListAsync();
			return roles;
		}
		public static async Task SaveGuildSelfRoleAsync(GuildSelfRole model)
		{
			using var Db = new NeiraLinkContext();
			Db.GuildSelfRoles.Add(model);
			await Db.SaveChangesAsync();
		}

		public static async Task ClearGuildSelfRoleAsync(ulong guildId)
		{
			using var Db = new NeiraLinkContext();
			var roles = Db.GuildSelfRoles.Where(g => g.GuildID == guildId);
			Db.RemoveRange(roles);
			await Db.SaveChangesAsync();
		}
		#endregion

		#region Destiny 2
		public static Gear GetExotic(string name)
		{
			using var Db = new NeiraLinkContext();
			return Db.Gears.AsNoTracking().FirstOrDefault(c => EF.Functions.Like(c.Name.ToLower(), $"%{Alias(name.ToLower())}%"));
		}

		public static Catalyst GetCatalyst(string name)
		{
			using var Db = new NeiraLinkContext();
			return Db.Catalysts.AsNoTracking().FirstOrDefault(c => EF.Functions.Like(c.WeaponName.ToLower(), $"%{Alias(name.ToLower())}%"));

		}
		public static List<Clan> GetDestinyClan(ulong id)
		{
			using var Db = new NeiraLinkContext();
			return Db.Clans.AsNoTracking().Include(C => C.Members).Where(G => G.GuildId == id).ToList();
		}
		#endregion

		#region Milestone
		/// <summary>
		/// Find milestone info from database, if not found return null
		/// </summary>
		/// <param name="milestoneName">milestone name like "last wish" or "lw" or "wish"</param>
		/// <returns>Milestone model or null</returns>
		public static async Task<Milestone> GetMilestoneAsync(string milestoneName)
		{
			using var Db = new NeiraLinkContext();

			return await Db.Milestones.AsNoTracking().FirstOrDefaultAsync(m => m.Alias == milestoneName.ToLower() || EF.Functions.Like(m.Name, $"%{milestoneName}%"));
		}

		/// <summary>
		/// Return list of all milestone in local db
		/// </summary>
		public static List<Milestone> GetAllMilestones()
		{
			using var Db = new NeiraLinkContext();
			return Db.Milestones.AsNoTracking().ToList();
		}
		#endregion

		private static string Alias(string name)
		{
			return name switch
			{
				"дарси" => "Д.А.Р.С.И.",
				"мида" => "MIDA",
				"сурос" => "SUROS",
				"морозники" => "M0р03ники",
				"топотуны" => "Т0п0тунЬI",
				_ => name,
			};
		}
	}
}
