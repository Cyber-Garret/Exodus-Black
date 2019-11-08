using BungieWorker.Bungie;

using System;
using System.Timers;

namespace BungieWorker
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
				Interval = Program.BungieTimer.TotalMilliseconds
			};
			BungieTimer.Elapsed += BungieTimer_Elapsed;
		}

		private void BungieTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				var updater = ClanUpdater.GetInstance();
				if (!(updater.UpdateClanBusy || updater.MemberCheckBusy || updater.UpdateMemberBusy))
				{
					if (!updater.UpdateClanBusy)
						updater.UpdateClans();
					if (!updater.MemberCheckBusy)
						updater.ClanMemberCheck();
					if (!updater.UpdateMemberBusy)
						updater.UpdateMembersLastPlayedTime();
				}
			}
			catch (Exception ex)
			{
				Logger.Log.Fatal(ex, "Fatal Exception in BungieTimer event");
			}
		}
	}
}
