using Bot.Models;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Core.Data
{
	internal static class MilestoneInfoData
	{
		private static readonly ConcurrentDictionary<string, MilestoneInfo> MilestonesData;
		static MilestoneInfoData()
		{
			var result = DataStorage.LoadJSONFromHDD<MilestoneInfo>(DataStorage.MilestonesInfoFolder);

			if (result != null)
				MilestonesData = result.ToConcurrentDictionary(k => k.Alias);
			else
				MilestonesData = new ConcurrentDictionary<string, MilestoneInfo>();
		}
		internal static List<MilestoneInfo> GetMilestonesByType(MilestoneType milestoneType)
		{
			return MilestonesData.Values.Where(m => m.MilestoneType == milestoneType).ToList();
		}

		internal static MilestoneInfo SearchMilestoneData(string name, MilestoneType type)
		{
			var searchName = MilestonesData.Keys.FirstOrDefault(key => key.Contains(name, StringComparison.InvariantCultureIgnoreCase));
			if (searchName != null)
			{
				MilestonesData.TryGetValue(searchName, out var milestone);

				if (milestone.MilestoneType == type)
					return milestone;
				else
					return null;
			}
			else
				return null;
		}
	}
}
