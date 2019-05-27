using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Web.BungieCache
{
	public class CacheService
	{
		private TimeSpan startTimeSpan = TimeSpan.Zero;
		private TimeSpan _15MinutePeriod = TimeSpan.FromMinutes(15);
		private ClanUpdater ClanUpdater;
		private MemberUpdater MemberUpdater;

		public void InitializeTimers()
		{
			ClanUpdater = new ClanUpdater();

			var ClanTimer = new Timer((e) =>
			{
				Console.WriteLine("Clan timer go work");
				ClanUpdater.UpdateAllClans();
			}, null, startTimeSpan, _15MinutePeriod);

			//TODO MEMBER UPDATER
		}
	}
}
