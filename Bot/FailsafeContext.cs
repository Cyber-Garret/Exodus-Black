using Bot.Models.Db;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

using Microsoft.EntityFrameworkCore;

using System.IO;

namespace Bot
{
	public class FailsafeContext : DbContext
	{
		public FailsafeContext()
		{
			Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			//var sqlitepath = Path.Combine(Directory.GetCurrentDirectory(), "UserData", "NeiraLink.db");
			//optionsBuilder.UseSqlite($"Data Source={sqlitepath}");
			optionsBuilder.UseMySql("Server=10.18.0.15;Database=NeiraLink;Uid=neira;Pwd=26256Garret;",
				mysqlOptions =>
				{
					mysqlOptions.ServerVersion(new System.Version(5, 7, 27), ServerType.MySql);
				});
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
		public DbSet<MilestoneUser> MilestoneUsers { get; set; }
	}
}
