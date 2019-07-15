using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models.Db
{
	public class ActiveRaid
	{
		[Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong MessageId { get; set; }
		[Required]
		public string Guild { get; set; }

		[Required]
		public int RaidInfoId { get; set; }
		public RaidInfo RaidInfo { get; set; }

		[MaxLength(1024), Required]
		public string Memo { get; set; }
		[Required]
		public DateTime DateExpire { get; set; }
		[Required]
		public ulong User1 { get; set; }
		[Required]
		public ulong User2 { get; set; }
		[Required]
		public ulong User3 { get; set; }
		[Required]
		public ulong User4 { get; set; }
		[Required]
		public ulong User5 { get; set; }
		[Required]
		public ulong User6 { get; set; }
	}
}
