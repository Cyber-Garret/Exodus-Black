using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Models
{
	public class Milestone
	{
		public ulong MessageId { get; set; }
		public ulong ChannelId { get; set; }
		public ulong GuildId { get; set; }
		public MilestoneInfo MilestoneInfo { get; set; }
		public string Note { get; set; }
		public ulong Leader { get; set; }
		public DateTimeOffset DateExpire { get; set; }
		public List<ulong> MilestoneUsers { get; set; } = new List<ulong>();
	}
}
