using Microsoft.EntityFrameworkCore;

using Core.Models.Db;

namespace Core
{
	public class FailsafeContext : DbContext
	{
		public FailsafeContext() { }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			//optionsBuilder.UseSqlServer("Server=159.69.21.188;Database=Neiralink;Trusted_Connection=False;User=sa;Password=yNRASo1FjL");
			optionsBuilder.UseSqlServer("Server=159.69.21.188;Database=Neiralink;Trusted_Connection=False;User=Failsafe;Password=gfkAD8EPc4~YLVpV;" +
				"MultipleActiveResultSets=True");
		}
		//Global
		public DbSet<Gear> Gears { get; set; }
		public DbSet<Guild> Guilds { get; set; }
		public DbSet<Destiny2Clan> Destiny2Clans { get; set; }
		public DbSet<Destiny2Clan_Member> Destiny2Clan_Members { get; set; }

		//Catalyst's
		public DbSet<Catalyst> Catalysts { get; set; }
	}
}
