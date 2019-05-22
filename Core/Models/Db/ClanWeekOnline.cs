using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models.Db
{
	public class ClanWeekOnline
	{
		public int Id { get; set; }
		public long ClanId { get; set; }
		public long MembershipType { get; set; }
		public string MembershipId { get; set; }
		public string IconPath { get; set; }
		public string Name { get; set; }
		public string BungieName { get; set; }
		public DateTimeOffset? ClanJoinDate { get; set; }
	}
}

