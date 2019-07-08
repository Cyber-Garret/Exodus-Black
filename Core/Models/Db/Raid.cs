using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models.Db
{
	public class Raid
	{
		public ulong Id { get; set; }
		public string Name { get; set; }
		public DateTime dueDate { get; set; }
		public ulong User1 { get; set; }
		public ulong User2 { get; set; }
		public ulong User3 { get; set; }
		public ulong User4 { get; set; }
		public ulong User5 { get; set; }
		public ulong User6 { get; set; }
	}
}
