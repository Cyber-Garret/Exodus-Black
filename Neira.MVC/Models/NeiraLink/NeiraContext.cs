using Microsoft.EntityFrameworkCore;

namespace Neira.MVC.Models.NeiraLink
{
	public class NeiraContext : DbContext
	{
		public NeiraContext(DbContextOptions<NeiraContext> options) : base(options) { }

		public DbSet<BotInfo> BotInfos { get; set; }
		//Destiny 2
		public DbSet<Clan> Clans { get; set; }
		public DbSet<Clan_Member> Clan_Members { get; set; }
	}
}
