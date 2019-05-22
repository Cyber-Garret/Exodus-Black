using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models.Db
{
	public class Destiny2Clan
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public DateTimeOffset CreateDate { get; set; }
		public string Motto { get; set; }
		public string About { get; set; }
		public long MemberCount { get; set; }
	}
}
