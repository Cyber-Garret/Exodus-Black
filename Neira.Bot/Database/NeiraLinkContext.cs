using Microsoft.EntityFrameworkCore;

namespace Neira.Bot.Database
{
	public class NeiraLinkContext : DbContext
	{
		public NeiraLinkContext()
		{
			Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(Program.config.ConnectionString);
		}

		//Web
		public virtual DbSet<BotInfo> BotInfos { get; set; }
		//Discord
		public virtual DbSet<Guild> Guilds { get; set; }
		public virtual DbSet<UserAccount> UserAccounts { get; set; }
		public virtual DbSet<GuildUserAccount> GuildUserAccounts { get; set; }
		public virtual DbSet<ADOnline> ADOnlines { get; set; }

		//Destiny 2
		public virtual DbSet<Gear> Gears { get; set; }
		public virtual DbSet<Catalyst> Catalysts { get; set; }
		public virtual DbSet<Clan> Clans { get; set; }
		public virtual DbSet<Clan_Member> Clan_Members { get; set; }
		//Milestones
		public virtual DbSet<Milestone> Milestones { get; set; }
		public virtual DbSet<ActiveMilestone> ActiveMilestones { get; set; }
		public virtual DbSet<MilestoneUser> MilestoneUsers { get; set; }
	}
}
