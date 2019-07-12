using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models.Db
{
	public class ActiveRaid
	{
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Key]
		public ulong MessageId { get; set; }
		public string Guild { get; set; }

		public int RaidInfoId { get; set; }
		public RaidInfo RaidInfo { get; set; }

		public DateTime DateExpire { get; set; }
		public ulong User1 { get; set; }
		public ulong User2 { get; set; }
		public ulong User3 { get; set; }
		public ulong User4 { get; set; }
		public ulong User5 { get; set; }
		public ulong User6 { get; set; }
	}
}
