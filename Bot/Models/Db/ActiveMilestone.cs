using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Models.Db
{
	public class ActiveMilestone
	{
		[Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong MessageId { get; set; }
		[Required]
		public ulong TextChannelId { get; set; }
		[Required]
		public ulong GuildId { get; set; }

		[Required]
		public int MilestoneId { get; set; }
		public Milestone Milestone { get; set; }

		[MaxLength(1000)]
		public string Memo { get; set; }
		[Required]
		public DateTime DateExpire { get; set; }
		[Required]
		public ulong Leader { get; set; }

		public List<MilestoneUser> MilestoneUsers { get; set; }
	}
}
