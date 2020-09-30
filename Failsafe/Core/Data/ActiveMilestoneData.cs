using Failsafe.Models;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Failsafe.Core.Data
{
	internal static class ActiveMilestoneData
	{
		private static readonly ConcurrentDictionary<ulong, Milestone> ActiveMilestones;
		static ActiveMilestoneData()
		{
			var result = DataStorage.LoadJSONFromHDD<Milestone>(DataStorage.MilestonesFolder);

			if (result != null)
				ActiveMilestones = result.ToConcurrentDictionary(k => k.MessageId);
			else
				ActiveMilestones = new ConcurrentDictionary<ulong, Milestone>();
		}

		/// <summary>
		/// Get all active milestones ordered by DateExpire for old to new
		/// </summary>
		/// <returns></returns>
		internal static List<Milestone> GetAllMilestones()
		{
			return ActiveMilestones.Values
				.OrderBy(r => r.DateExpire)
				.ToList();
		}

		internal static Milestone GetMilestone(ulong messageId)
		{
			ActiveMilestones.TryGetValue(messageId, out Milestone milestone);

			return milestone;
		}

		internal static void AddMilestone(Milestone milestone)
		{
			if (ActiveMilestones.TryAdd(milestone.MessageId, milestone))
				DataStorage.SaveObject(milestone, Path.Combine(DataStorage.MilestonesFolder, $"{milestone.MessageId}.json"), true);
		}

		internal static void RemoveMilestone(ulong MessageId)
		{
			//remove from dictionary
			if (ActiveMilestones.TryRemove(MessageId, out Milestone value))
				//remove file
				DataStorage.RemoveObject(Path.Combine(DataStorage.MilestonesFolder, $"{value.MessageId}.json"));
		}

		internal static void SaveMilestones(params ulong[] ids)
		{
			foreach (var id in ids)
			{
				DataStorage.SaveObject(GetMilestone(id), Path.Combine(DataStorage.MilestonesFolder, $"{id}.json"), true);
			}
		}

		internal static void SaveMilestones()
		{
			foreach (var id in ActiveMilestones.Keys)
			{
				SaveMilestones(id);
			}
		}
	}
}
