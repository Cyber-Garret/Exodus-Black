using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Core.Models.Db
{
	public class FailsafeContext : DbContext
	{
		public FailsafeContext()
		{
			Database.EnsureCreated();
			if (!Database.EnsureCreated())
				Database.Migrate();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer("Server=159.69.21.188;Database=Neiralink;Trusted_Connection=False;User=sa;Password=yNRASo1FjL");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Guild>().Property(g => g.ID).ValueGeneratedNever();
			modelBuilder.Entity<Guild>().Property(g => g.NotificationChannel).HasDefaultValue(0).ValueGeneratedNever();
			modelBuilder.Entity<Guild>().Property(g => g.LoggingChannel).HasDefaultValue(0).ValueGeneratedNever();
			modelBuilder.Entity<Guild>().Property(g => g.EnableLogging).HasDefaultValue(false).ValueGeneratedNever();
			modelBuilder.Entity<Guild>().Property(g => g.EnableNotification).HasDefaultValue(false).ValueGeneratedNever();

			modelBuilder.Entity<Destiny2Clan>().Property(c => c.Id).ValueGeneratedNever();
		}

		public DbSet<Gear> Gears { get; set; }
		public DbSet<Guild> Guilds { get; set; }
		public DbSet<Destiny2Clan> Destiny2Clans { get; set; }
		//public DbSet<ClanWeekOnline> ClanWeekOnlines { get; set; }
	}
}
