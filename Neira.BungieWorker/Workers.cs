using Neira.BungieWorker.Bungie;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Timers;

namespace Neira.BungieWorker
{
	internal class Workers
	{
		private readonly Timer BungieTimer;
		public Workers()
		{
			BungieTimer = new Timer
			{
				Enabled = true,
				AutoReset = true,
				Interval = TimeSpan.FromMinutes(1).TotalMilliseconds
			};
			BungieTimer.Elapsed += ClanTimer_ElapsedAsync;
		}

		private async void ClanTimer_ElapsedAsync(object sender, ElapsedEventArgs e)
		{
			var updater = ClanUpdater.GetInstance();
			if (!updater.UpdateClanBusy)
				updater.UpdateClans();
			if (!updater.MemberCheckBusy)
				await updater.ClanMemberCheckAsync();
			if (!updater.UpdateMemberBusy)
				await updater.UpdateMembersLastPlayedTime();
		}
	}
}
