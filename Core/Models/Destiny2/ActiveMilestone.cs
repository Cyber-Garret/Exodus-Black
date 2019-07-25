using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models.Destiny2
{
	public class ActiveMilestone
	{
		[Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong MessageId { get; set; }
		[Required]
		public string GuildName { get; set; }

		[Required]
		public int MilestoneId { get; set; }
		public Milestone Milestone { get; set; }

		[MaxLength(1000)]
		public string Memo { get; set; }
		[Required]
		public DateTime DateExpire { get; set; }
		[Required]
		public ulong User1 { get; set; }
		public ulong User2 { get; set; } = 0;
		public ulong User3 { get; set; } = 0;
		public ulong User4 { get; set; } = 0;
		public ulong User5 { get; set; } = 0;
		public ulong User6 { get; set; } = 0;
	}
}
