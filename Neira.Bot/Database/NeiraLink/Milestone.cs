using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neira.Bot.Database
{
	public class Milestone
	{
		[Key]
		public int Id { get; set; }
		[MaxLength(100)]
		public string Name { get; set; }
		[MaxLength(50)]
		public string Alias { get; set; }
		[MaxLength(1024)]
		public string PreviewDesc { get; set; }
		[MaxLength(50)]
		public string Type { get; set; }
		[MaxLength(1000)]
		public string Icon { get; set; }

		public List<ActiveMilestone> ActiveMilestones { get; set; }
	}

	public class MilestoneUser
	{
		[Key]
		public int Id { get; set; }
		public ulong MessageId { get; set; }
		public ActiveMilestone ActiveMilestone { get; set; }
		public ulong UserId { get; set; }
	}

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

	public class OldActiveMilestone
	{
		[Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong MessageId { get; set; }
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
		public ulong User1 { get; set; }
		public ulong User2 { get; set; } = 0;
		public ulong User3 { get; set; } = 0;
		public ulong User4 { get; set; } = 0;
		public ulong User5 { get; set; } = 0;
		public ulong User6 { get; set; } = 0;
	}
}
