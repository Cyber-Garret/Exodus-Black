using System.Collections.Generic;

using Neiralink.Models;

namespace WebSite.ViewModels
{
	public class MilestoneEditViewModel
	{
		public MilestoneInfo Info { get; set; }
		public List<MilestoneInfoLocale> Locales { get; set; } = new List<MilestoneInfoLocale>();
	}
}
