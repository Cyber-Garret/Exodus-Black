﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neira.Bot.Database
{
	public class Milestone
	{
		[Key]
		public byte Id { get; set; }
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
		public byte MaxSpace { get; set; }

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
		public ulong GuildId { get; set; }
		[Required]
		public byte MilestoneId { get; set; }
		public Milestone Milestone { get; set; }

		[MaxLength(1000)]
		public string Memo { get; set; }
		[Required]
		public ulong Leader { get; set; }
		[Required]
		public DateTime CreateDate { get; set; }

		public List<MilestoneUser> MilestoneUsers { get; set; }
	}
}