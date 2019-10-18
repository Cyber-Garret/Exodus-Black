using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Web.Models
{
	public class NeiraContext : DbContext
	{
		public DbSet<BotInfo> BotInfos { get; set; }
		public NeiraContext(DbContextOptions<NeiraContext> options) : base(options)
		{
			Database.EnsureCreated();
		}
	}
}
