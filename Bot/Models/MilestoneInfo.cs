using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Models
{
	public class MilestoneInfo
	{
		public string Name { get; set; }
		public string Alias { get; set; }
		public string Type { get; set; }
		public string Icon { get; set; }
		public byte MaxSpace { get; set; }
		public MilestoneType MilestoneType { get; set; }
	}

	public enum MilestoneType
	{
		Raid,
		Strike,
		Other
	}
}
