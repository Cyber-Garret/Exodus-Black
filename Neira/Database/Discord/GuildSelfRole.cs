using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Database
{
	public class GuildSelfRole
	{
		[Key]
		public int RowID { get; set; }
		public ulong GuildID { get; set; }
		public ulong EmoteID { get; set; }
		public ulong RoleID { get; set; }
	}
}
