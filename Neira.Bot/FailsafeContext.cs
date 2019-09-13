using Microsoft.EntityFrameworkCore;

using Neira.Bot.Models.Db;

using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

using System;

namespace Neira.Bot
{
	public class FailsafeContext : DbContext
	{
		public FailsafeContext()
		{
			Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			var connection = $"Server={Program.config.MySQL.Server};Database={Program.config.MySQL.Database};Uid={Program.config.MySQL.User};Pwd={Program.config.MySQL.Password};";
			optionsBuilder.UseMySql(connection,
				mysqlOptions =>
				{
					mysqlOptions.ServerVersion(new Version(5, 7, 27), ServerType.MySql);
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
