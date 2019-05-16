using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Core.Models.Db
{
	public class FailsafeContext : DbContext
	{
		public DbSet<Gear> Gears { get; set; }
		public DbSet<Guild> Guilds { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder Options)
		{
			//string DbLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"/UserData/Neuromatrix.db";
			string DbLocation = @"Neuromatrix.db";
			Options.UseSqlite($"DataSource={DbLocation}", b => b.MigrationsAssembly("Web"));
			base.OnConfiguring(Options);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Guild>().Property(g => g.ID).ValueGeneratedNever();
			modelBuilder.Entity<Guild>().Property(g => g.NotificationChannel).HasDefaultValue(0).ValueGeneratedNever();
			modelBuilder.Entity<Guild>().Property(g => g.LoggingChannel).HasDefaultValue(0).ValueGeneratedNever();
			modelBuilder.Entity<Guild>().Property(g => g.EnableLogging).HasDefaultValue(false).ValueGeneratedNever();
			modelBuilder.Entity<Guild>().Property(g => g.EnableNotification).HasDefaultValue(false).ValueGeneratedNever();
		}
	}
}
