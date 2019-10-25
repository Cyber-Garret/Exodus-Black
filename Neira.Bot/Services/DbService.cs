using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Neira.Bot.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neira.Bot.Services
{
	public class DbService
	{
		private readonly NeiraContext Db;
		public DbService(NeiraContext neiraContext)
		{
			Db = neiraContext;
		}

		internal async Task<Gear> GetExoticGear(string name)
		{
			//Get random Exotic gear
			if (name.ToLower() == "любой")
			{
				var r = new Random();
				int randomId = r.Next(1, Db.Gears.Count());
				return await Db.Gears.AsNoTracking().Skip(randomId).Take(1).FirstOrDefaultAsync();
			}
			else
			{
				return await Db.Gears.AsNoTracking().Where(c => c.Name.IndexOf(Alias(name), StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefaultAsync();
			}
		}

		internal async Task<Catalyst> GetWeaponCatalyst(string name)
		{
			//Get random weapon catalyst
			if (name.ToLower() == "любой")
			{
				var r = new Random();
				int randomId = r.Next(1, Db.Catalysts.Count());
				return await Db.Catalysts.AsNoTracking().Skip(randomId).Take(1).FirstOrDefaultAsync();
			}
			else
			{
				return await Db.Catalysts.AsNoTracking().Where(c => c.WeaponName.IndexOf(Alias(name), StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefaultAsync();
			}
		}

		internal async Task<List<Guild>> GetAllGuildsAsync()
		{
			var guilds = await Db.Guilds.ToListAsync();
			return guilds;
		}

		internal async Task<Guild> GetGuildAccountAsync(ulong guildId)
		{
			if (Db.Guilds.Any(g => g.Id == guildId))
				return Db.Guilds.Single(G => G.Id == guildId);
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

		internal async Task SaveGuildAccountAsync(ulong GuildId, Guild guildAccount)
		{
			if (Db.Guilds.Any(G => G.Id == GuildId))
				Db.Guilds.Update(guildAccount);
			else
			{
				var newGuild = new Guild
				{
					Id = GuildId
				};

				Db.Guilds.Add(newGuild);

			}
			await Db.SaveChangesAsync();
		}

		internal async void SaveWelcomeMessage(ulong GuildId, string value)
		{
			try
			{
				var GuildData = Db.Guilds.Single(g => g.Id == GuildId);
				GuildData.WelcomeMessage = value;
				Db.Guilds.Update(GuildData);
				Db.SaveChanges();
			}
			catch (InvalidOperationException ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Critical, "SaveWelcomeMessage[InvalidOperation]", ex.Message));
			}
			catch (DbUpdateException ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Critical, "SaveWelcomeMessage[DbUpdate]", ex.Message));
			}

		}

		internal async Task<Milestone> GetMilestone(string milestoneName)
		{
			var milestone = await Db.Milestones.Where(r =>
			r.Name.IndexOf(milestoneName, StringComparison.CurrentCultureIgnoreCase) != -1 ||
			r.Alias.IndexOf(milestoneName, StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefaultAsync();
			return milestone;
		}

		internal async Task<List<Milestone>> GetAllMilestone()
		{
			return await Db.Milestones.AsNoTracking().ToListAsync();
		}

		internal async Task<List<Milestone>> GetAllMilestones()
		{
			var milestones = await Db.Milestones.ToListAsync();
			return milestones;
		}

		internal async Task<List<ActiveMilestone>> GetAllActiveMilestones()
		{
			return await Db.ActiveMilestones
				.Include(r => r.Milestone)
				.Include(ac => ac.MilestoneUsers)
				.OrderBy(o => o.DateExpire)
				.ToListAsync();
		}

		internal async Task<ActiveMilestone> GetActiveMilestone(ulong msgId)
		{
			var activeMilestone = await Db.ActiveMilestones.Include(r => r.Milestone).Where(r => r.MessageId == msgId).FirstOrDefaultAsync();
			return activeMilestone;
		}

		internal async Task SaveActiveMilestone(ActiveMilestone activeMilestone)
		{
			if (Db.ActiveMilestones.Any(r => r.MessageId == activeMilestone.MessageId))
				Db.ActiveMilestones.Update(activeMilestone);
			else
				Db.ActiveMilestones.Add(activeMilestone);

			await Db.SaveChangesAsync();

		}

		internal async Task<List<Clan>> GetDestinyClan(ulong id)
		{
			return await Db.Clans.AsNoTracking().Include(C => C.Members).Where(G => G.GuildId == id).ToListAsync();
		}

		internal async Task<DailyResult> GetDailyAsync(ulong userId)
		{
			if (Db.UserAccounts.Any(u => u.Id == userId))
			{
				var user = await Db.UserAccounts.SingleAsync(u => u.Id == userId);
				var difference = DateTime.UtcNow - user.LastDaily.AddDays(1);

				if (difference.TotalHours < 0)
					return new DailyResult { Success = false, RefreshTimeSpan = difference };
				else
				{
					user.Glimmer += Global.DailyGlimmerGain;
					user.LastDaily = DateTime.UtcNow;

					Db.UserAccounts.Update(user);
					await Db.SaveChangesAsync();

					return new DailyResult { Success = true };
				}
			}
			else
			{
				var newUser = new UserAccount
				{
					Id = userId,
					Glimmer = Global.DailyGlimmerGain
				};

				Db.UserAccounts.Add(newUser);
				await Db.SaveChangesAsync();
				return new DailyResult { Success = true };
			}

		}

		internal async Task<DailyResult> GetRepAsync(SocketGuildUser user)
		{
			if (Db.GuildUserAccounts.Any(u => u.UserId == user.Id && u.GuildId == user.Guild.Id))
			{
				var account = await Db.GuildUserAccounts.SingleAsync(u => u.UserId == user.Id && u.GuildId == user.Guild.Id);
				var difference = DateTime.UtcNow - account.LastRep.AddDays(1);

				if (difference.TotalHours < 0)
					return new DailyResult { Success = false, RefreshTimeSpan = difference };
				else
				{
					account.LastRep = DateTime.UtcNow;
					Db.GuildUserAccounts.Update(account);
					await Db.SaveChangesAsync();
					return new DailyResult { Success = true };
				}
			}
			else
			{
				var newAccount = new GuildUserAccount
				{
					UserId = user.Id,
					GuildId = user.Guild.Id,
					LastRep = DateTime.UtcNow

				};
				Db.GuildUserAccounts.Add(newAccount);
				await Db.SaveChangesAsync();
				return new DailyResult { Success = true };
			}
		}
		internal async Task AddRepAsync(SocketGuildUser user)
		{
			var account = await Db.GuildUserAccounts.SingleOrDefaultAsync(u => u.UserId == user.Id && u.GuildId == user.Guild.Id);

			if (account != null)
			{
				account.Reputation++;
				Db.GuildUserAccounts.Update(account);
				await Db.SaveChangesAsync();
			}
			else
			{
				var newAccount = new GuildUserAccount
				{
					UserId = user.Id,
					GuildId = user.Guild.Id,
					Reputation = 1
				};
				Db.GuildUserAccounts.Add(newAccount);
				await Db.SaveChangesAsync();
			}
		}

		internal List<UserAccount> GetFilteredAccounts(Func<UserAccount, bool> filter)
		{
			return Db.UserAccounts.Where(filter).ToList();
		}

		internal async Task<UserAccount> GetUserAccountAsync(IUser user)
		{
			//Check if user exist
			if (Db.UserAccounts.Any(u => u.Id == user.Id))
				return await Db.UserAccounts.SingleAsync(u => u.Id == user.Id);
			else
			{
				var newUser = new UserAccount
				{
					Id = user.Id
				};
				Db.UserAccounts.Add(newUser);
				await Db.SaveChangesAsync();

				return newUser;
			}
		}
		internal async Task SaveUserAccountAsync(UserAccount userAccount)
		{
			try
			{
				if (Db.UserAccounts.Any(u => u.Id == userAccount.Id))
				{
					Db.UserAccounts.Update(userAccount);
					await Db.SaveChangesAsync();
				}
				else
				{
					Db.UserAccounts.Add(userAccount);
					await Db.SaveChangesAsync();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw;
			}
		}

		internal struct DailyResult
		{
			public bool Success;
			public TimeSpan RefreshTimeSpan;
		}

		private string Alias(string name)
		{
			switch (name)
			{
				case "дарси":
					return "Д.А.Р.С.И.";
				case "мида":
					return "MIDA";
				case "сурос":
					return "SUROS";
				case "морозники":
					return "M0р03ники";
				case "топотуны":
					return "Т0п0тунЬI";
				default:
					return name;
			}
		}
	}
}
