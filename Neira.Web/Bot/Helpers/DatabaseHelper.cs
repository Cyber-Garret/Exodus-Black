using Discord;

using Microsoft.EntityFrameworkCore;

using Neira.Web.Database;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Web.Bot.Helpers
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
			using (var Db = new NeiraLinkContext())
			{
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
		}

		public static async Task SaveGuildAccountAsync(Guild guildAccount)
		{
			using (var Db = new NeiraLinkContext())
			{
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
		}

		public static async Task RemoveGuildAccountAsync(ulong guildId)
		{
			using (var Db = new NeiraLinkContext())
			{
				if (Db.Guilds.Any(g => g.Id == guildId))
				{
					var guild = Db.Guilds.First(g => g.Id == guildId);
					Db.Guilds.Remove(guild);
					await Db.SaveChangesAsync();
				}
			}
		}
		#endregion

		#region Destiny 2
		public static Gear GetExotic(string name)
		{
			using var Db = new NeiraLinkContext();
			//We create dictionary because like in SQlite bugged
			Dictionary<string, Gear> gears = new Dictionary<string, Gear>();
			foreach (var item in Db.Gears.AsNoTracking())
				gears.Add(item.Name.ToLowerInvariant(), item);

			var gear = gears.FirstOrDefault(g => EF.Functions.Like(g.Key, $"%{Alias(name).ToLowerInvariant()}%"));
			//var gear = Db.Gears.AsNoTracking().FirstOrDefault(c => EF.Functions.Like(c.Name, $"%{Alias(name)}%"));
			return gear.Value;
		}

		public static Catalyst GetCatalyst(string name)
		{
			using var Db = new NeiraLinkContext();
			//We create dictionary because like in SQlite bugged
			Dictionary<string, Catalyst> catalysts = new Dictionary<string, Catalyst>();
			foreach (var item in Db.Catalysts.AsNoTracking())
				catalysts.Add(item.WeaponName.ToLowerInvariant(), item);

			var catalyst = catalysts.FirstOrDefault(g => EF.Functions.Like(g.Key, $"%{Alias(name).ToLowerInvariant()}%"));
			//var catalyst = Db.Catalysts.AsNoTracking().FirstOrDefault(c => EF.Functions.Like(c.WeaponName.ToLower(), $"%{Alias(name)}%"));
			return catalyst.Value;
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
		public static Milestone GetMilestone(string milestoneName)
		{
			using var Db = new NeiraLinkContext();
			//We create dictionary because like in SQlite bugged
			Dictionary<string, Milestone> milestones = new Dictionary<string, Milestone>();
			foreach (var item in Db.Milestones.AsNoTracking())
				milestones.Add(item.Name.ToLowerInvariant(), item);

			var milestone = milestones.FirstOrDefault(m => m.Value.Alias == milestoneName.ToLower() || EF.Functions.Like(m.Value.Name, $"%{milestoneName}%"));

			//return await Db.Milestones.AsNoTracking().FirstOrDefaultAsync(m => m.Alias == milestoneName.ToLower() || EF.Functions.Like(m.Name, $"%{milestoneName}%"));
			return milestone.Value;
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

		#region Economy
		public static List<UserAccount> GetFilteredAccounts(Func<UserAccount, bool> filter)
		{
			using var Db = new NeiraLinkContext();
			return Db.UserAccounts.Where(filter).ToList();
		}
		/// <summary>
		/// Find or create user global account
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static async Task<UserAccount> GetUserAccountAsync(IUser user)
		{
			using var db = new NeiraLinkContext();
			//Check if user exist
			if (db.UserAccounts.Any(u => u.Id == user.Id))
				return await db.UserAccounts.SingleAsync(u => u.Id == user.Id);
			else
			{
				var newUser = new UserAccount
				{
					Id = user.Id
				};
				db.UserAccounts.Add(newUser);
				await db.SaveChangesAsync();

				return newUser;
			}
		}

		/// <summary>
		/// Update or add user global account
		/// </summary>
		/// <param name="userAccount"></param>
		/// <returns></returns>
		public static async Task SaveUserAccountAsync(UserAccount userAccount)
		{
			using var Db = new NeiraLinkContext();
			if (Db.UserAccounts.Any(u => u.Id == userAccount.Id))
				Db.UserAccounts.Update(userAccount);
			else
				Db.UserAccounts.Add(userAccount);
			await Db.SaveChangesAsync();
		}
		#endregion

		#region Reputation
		/// <summary>
		/// Find or create discord guild user account
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static async Task<GuildUserAccount> GetGuildUserAccountAsync(IGuildUser user)
		{
			using var Db = new NeiraLinkContext();
			//Check if user exist
			if (Db.GuildUserAccounts.Any(u => u.UserId == user.Id && u.GuildId == user.GuildId))
				return await Db.GuildUserAccounts.SingleAsync(u => u.UserId == user.Id && u.GuildId == user.GuildId);
			else
			{
				var newUser = new GuildUserAccount
				{
					UserId = user.Id,
					GuildId = user.Guild.Id
				};
				Db.GuildUserAccounts.Add(newUser);
				await Db.SaveChangesAsync();

				return newUser;
			}
		}

		/// <summary>
		/// Update or add discord guild user account
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static async Task SaveGuildUserAccountAsync(GuildUserAccount user)
		{
			using var Db = new NeiraLinkContext();
			if (Db.GuildUserAccounts.Any(u => u.UserId == user.UserId && u.GuildId == user.GuildId))
				Db.GuildUserAccounts.Update(user);
			else
				Db.GuildUserAccounts.Add(user);
			await Db.SaveChangesAsync();
		}
		#endregion

		public struct DailyResult
		{
			public bool Success;
			public TimeSpan RefreshTimeSpan;
		}

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
