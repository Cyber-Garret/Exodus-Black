using Microsoft.EntityFrameworkCore;

namespace Neira.Web.Models.NeiraLink
{
	public class NeiraContext : DbContext
	{
		protected override void OnConfiguring(DbContextOptionsBuilder builder)
		{
			builder.UseSqlServer("Server=176.36.73.241;Database=NeiraLinkTest;User=sa;Password=26256Garret;MultipleActiveResultSets=true;");
		}

		public virtual DbSet<BotInfo> BotInfos { get; set; }
		//Destiny 2
		public virtual DbSet<Clan> Clans { get; set; }
		public virtual DbSet<Clan_Member> Clan_Members { get; set; }
		public virtual DbSet<Clan_Member_Stat> Clan_Member_Stats { get; set; }
	}
}
