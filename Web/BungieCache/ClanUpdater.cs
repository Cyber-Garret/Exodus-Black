using API.Bungie;
using Core.Models.Db;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Web.BungieCache
{
	internal class ClanUpdater
	{
		internal void UpdateAllClans()
		{
			var queue = new Queue<long>();
			var bungieApi = new BungieApi();

			using (FailsafeContext failsafe = new FailsafeContext())
			{
				//Grab all Destiny Clan ID and add to Queue
				foreach (var item in failsafe.Destiny2Clans.Select(C => new { C.Id }))
				{
					queue.Enqueue(item.Id);
				}

				while (queue.Count > 0)
				{
					var ClanId = queue.Dequeue();
					var ClanInfoDb = failsafe.Destiny2Clans.First(C => C.Id == ClanId);
					try
					{
						var ClanInfoBungie = bungieApi.GetGroupResult(ClanId);

						if (ClanInfoBungie == null)
							failsafe.Destiny2Clans.Remove(ClanInfoDb);
						else
						{
							ClanInfoDb.Name = ClanInfoBungie.Response.Detail.Name;
							ClanInfoDb.Motto = ClanInfoBungie.Response.Detail.Motto;
							ClanInfoDb.About = ClanInfoBungie.Response.Detail.About;
							ClanInfoDb.MemberCount = ClanInfoBungie.Response.Detail.MemberCount;
						}
						failsafe.Destiny2Clans.Update(ClanInfoDb);
						failsafe.SaveChanges();
					}
					catch (Exception ex) { Console.WriteLine(ex.ToString()); }
				}
			}

		}
	}
}
