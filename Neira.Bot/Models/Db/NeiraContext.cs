using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neira.Bot.Models.Db
{
	public class NeiraContext : DbContext
	{
		public NeiraContext(DbContextOptions<NeiraContext> options)
			: base(options)
		{
			Database.EnsureCreated();
		}

		//Web
		public DbSet<BotInfo> BotInfos { get; set; }
		//Discord
		public DbSet<Guild> Guilds { get; set; }
		public DbSet<UserAccount> UserAccounts { get; set; }
		public DbSet<GuildUserAccount> GuildUserAccounts { get; set; }

		//Destiny 2
		public DbSet<Gear> Gears { get; set; }
		public DbSet<Catalyst> Catalysts { get; set; }
		public DbSet<Clan> Clans { get; set; }
		public DbSet<Clan_Member> Clan_Members { get; set; }
		public DbSet<Milestone> Milestones { get; set; }
		public DbSet<ActiveMilestone> ActiveMilestones { get; set; }
		public DbSet<MilestoneUser> MilestoneUsers { get; set; }
	}
}
