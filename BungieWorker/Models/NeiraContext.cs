using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BungieWorker.Models
{
	public class NeiraContext : DbContext
	{
		public virtual DbSet<Clan> Clans { get; set; }
		public virtual DbSet<Clan_Member> Clan_Members { get; set; }
		protected override void OnConfiguring(DbContextOptionsBuilder options) =>
			options.UseSqlServer("Server=176.36.73.241;Database=NeiraLinkTest;User Id=Neira;Password=26256Maokai;MultipleActiveResultSets=true;");
	}
}
