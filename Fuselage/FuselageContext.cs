using Fuselage.Models;

using Microsoft.EntityFrameworkCore;

namespace Fuselage
{
	public class FuselageContext : DbContext
	{
		public FuselageContext()
		{
			Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseMySql("Server=neira.app;Database=NeiralinkTest;Uid=root;Pwd=qF6XiKwyQsUuB19cxmtG;");
		}
		/// <summary>
		/// Localized random welcomes
		/// </summary>
		public DbSet<Welcome> Welcomes { get; set; }

		public DbSet<Catalyst> Catalysts { get; set; }
	}
}
