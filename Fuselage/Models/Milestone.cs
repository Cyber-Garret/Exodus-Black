
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Fuselage.Enums;

namespace Fuselage.Models
{
	public class Milestone
	{
		[Key]
		public int Id { get; set; }

		[MaxLength(2048), Display(Name = "Icon URL")]
		public string Icon { get; set; }

		[Display(Name = "Maximum users in battle group"), Range(2, 20, ErrorMessage = "Error: space range must between 2 and 20.")]
		public byte MaxSpace { get; set; }

		[Display(Name = "Milestone type")]
		public MilestoneType Type { get; set; }

		//TODO: store and get color from DB
		//[Display(Name = "Milestone color in Discord")]
		//public Color Color { get; set; }

		public IEnumerable<MilestoneLocale> MilestoneLocales { get; set; }
	}
}
