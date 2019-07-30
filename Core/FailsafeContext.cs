using Microsoft.EntityFrameworkCore;

using Core.Models.Discord;
using Core.Models.Destiny2;

namespace Core
{
	public class FailsafeContext : DbContext
	{
		public FailsafeContext() { }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite("Data Source=Failsafe.db");
			//optionsBuilder.UseSqlServer("Server=159.69.21.188;Database=Neiralink;Trusted_Connection=False;User=Failsafe;Password=gfkAD8EPc4~YLVpV;" +
			//	"MultipleActiveResultSets=True");
		}

		//Discord
		public DbSet<Guild> Guilds { get; set; }

		//Destiny 2
		public DbSet<Gear> Gears { get; set; }
		public DbSet<Catalyst> Catalysts { get; set; }
		public DbSet<Clan> Clans { get; set; }
		public DbSet<Clan_Member> Clan_Members { get; set; }
		public DbSet<Milestone> Milestones { get; set; }
		public DbSet<ActiveMilestone> ActiveMilestones { get; set; }
	}
}
