using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Web.BungieCache
{
	public class CacheService
	{
		private Timer ClanTimer;
		private Timer MemberTimer;
		private readonly double FifteenMinute = TimeSpan.FromMinutes(15).TotalMilliseconds;
		private readonly double OneHour = TimeSpan.FromHours(1).TotalMilliseconds;

		public void InitializeTimers()
		{
			ClanTimer = new Timer
			{
				AutoReset = true,
				Enabled = true,
				Interval = FifteenMinute
			};
			ClanTimer.Elapsed += ClanTimer_Elapsed;

			MemberTimer = new Timer
			{
				AutoReset = true,
				Enabled = true,
				Interval = OneHour
			};
			MemberTimer.Elapsed += MemberTimer_Elapsed;
		}

		private void ClanTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			ClanUpdater updater = new ClanUpdater();
			updater.UpdateAllClans();
			updater.ClanMemberCheck();
		}

		private void MemberTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			MemberUpdater updater = new MemberUpdater();
			updater.UpdateMembersLastPlayedTime();
		}
	}
}
