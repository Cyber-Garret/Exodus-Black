using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Database
{
	public class GuildSelfRole
	{
		public ulong GuildId { get; set; }
		public Guild Guild { get; set; }
		public ulong EmoteId { get; set; }
		public ulong RoleId { get; set; }
	}
}
