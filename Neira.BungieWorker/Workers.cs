using Neira.BungieWorker.Bungie;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Neira.BungieWorker
{
	internal class Workers
	{
		private readonly Timer ClanTimer;
		private readonly Timer MemberTimer;
		public Workers()
		{
			ClanTimer = new Timer
			{
				Enabled = true,
				AutoReset = true,
				Interval = TimeSpan.FromMinutes(1).TotalMilliseconds
			};
			ClanTimer.Elapsed += ClanTimer_Elapsed;

			Logger.Log.Information("Initialize Clan Timer");

			MemberTimer = new Timer
			{
				Enabled = true,
				AutoReset = true,
				Interval = TimeSpan.FromMinutes(1).TotalMilliseconds
			};
			MemberTimer.Elapsed += MemberTimer_Elapsed;

			Logger.Log.Information("Initialize Member Timer");
		}

		private void ClanTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			var updater = new ClanUpdater();
			updater.UpdateAllClans();
			updater.ClanMemberCheck();
		}
		private void MemberTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			var updater = new MemberUpdater();
			updater.UpdateMembersLastPlayedTime();
		}
	}
}
