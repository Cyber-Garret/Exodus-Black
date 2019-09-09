using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bot.Models.Db
{
	public class MilestoneUser
	{
		[Key]
		public int Id { get; set; }
		public ulong MessageId { get; set; }
		public ActiveMilestone ActiveMilestone { get; set; }
		public ulong UserId { get; set; }
	}
}
