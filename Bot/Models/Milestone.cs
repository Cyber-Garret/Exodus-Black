using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Models
{
	public class Milestone
	{
		public ulong MessageId { get; set; }
		public ulong GuildId { get; set; }
		public MilestoneInfo MilestoneInfo { get; set; }
		public string Note { get; set; }
		public ulong Leader { get; set; }
		public DateTime DateExpire { get; set; }
		public List<MilestoneUser> MilestoneUsers { get; set; } = new List<MilestoneUser>();
	}

	public class MilestoneUser
	{
		public byte? Place { get; set; }
		public ulong UserId { get; set; }
	}
}
