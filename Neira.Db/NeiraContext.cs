using Microsoft.EntityFrameworkCore;

using Neira.Db.Models;
using Newtonsoft.Json;

using System;
using System.IO;

namespace Neira.Db
{
	public class NeiraContext : DbContext
	{
		private Credential credential;
		public NeiraContext()
		{
			try
			{
				Database.EnsureCreated();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}, Source: Neira Database] Message: {ex.Message}");
			}
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			//Load Db credentials
			var json = File.ReadAllText("credential.json");
			credential = JsonConvert.DeserializeObject<Credential>(json);

			var connection = $"Server={credential.Server};Database={credential.Database};User Id={credential.User};Password={credential.Password};MultipleActiveResultSets=true;";
			//var connection = "Server=10.18.0.15;Database=datab;User Id=login;Password=pass;";
			optionsBuilder.UseSqlServer(connection);
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
