using Neira.Web.Models.NeiraLink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Web.ViewModels
{
	public class GuardianViewModel
	{
		public Clan_Member GuardianInfo { get; set; }
		public IEnumerable<Clan_Member_Stat> GuardianPlayedStat { get; set; }
	}
}
