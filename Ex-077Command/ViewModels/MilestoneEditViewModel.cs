using Neiralink.Models;

using System.Collections.Generic;

namespace WebSite.ViewModels
{
	public class MilestoneViewModel
	{
		public MilestoneInfo Info { get; set; }
		public List<MilestoneInfoLocale> Locales { get; set; } = new List<MilestoneInfoLocale>();
	}
}
