﻿using Bot.Models.Db;

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
			var sqlitepath = Path.Combine(Directory.GetCurrentDirectory(), "UserData", "NeiraLink.db");
			optionsBuilder.UseSqlite($"Data Source={sqlitepath}");
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
