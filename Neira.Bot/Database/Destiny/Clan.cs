using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neira.Bot.Database
{
	public class Clan
	{
		[Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long Id { get; set; }
		public string Name { get; set; }
		public DateTimeOffset CreateDate { get; set; }
		public string Motto { get; set; }
		public string About { get; set; }
		public long MemberCount { get; set; }
		public ulong? GuildId { get; set; }
		public List<Clan_Member> Members { get; set; }

	}
}
