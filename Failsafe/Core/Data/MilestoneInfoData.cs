using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Failsafe.Models;
using Failsafe.Models.Enums;

namespace Failsafe.Core.Data
{
	internal static class MilestoneInfoData
	{
		private static readonly ConcurrentDictionary<string, MilestoneInfo> MilestonesData;
		static MilestoneInfoData()
		{
			var result = DataStorage.LoadJSONFromHDD<MilestoneInfo>(DataStorage.MilestonesInfoFolder);

			MilestonesData = result != null ? result.ToConcurrentDictionary(k => k.Alias) : new ConcurrentDictionary<string, MilestoneInfo>();
		}
		internal static List<MilestoneInfo> GetMilestonesByType(MilestoneType milestoneType)
		{
			return MilestonesData.Values.Where(m => m.MilestoneType == milestoneType).ToList();
		}

		/// <summary>
		/// Search any Milestone info with type
		/// </summary>
		/// <param name="name">milestone alias</param>
		/// <returns></returns>
		internal static MilestoneInfo SearchMilestoneData(string name, MilestoneType type)
		{
			var searchName = MilestonesData.Keys.FirstOrDefault(key => key.Contains(name, StringComparison.InvariantCultureIgnoreCase));

			if (searchName == null) return null;

			MilestonesData.TryGetValue(searchName, out var milestone);

			return milestone != null && milestone.MilestoneType == type ? milestone : null;

		}

		/// <summary>
		/// Search any Milestone info without type
		/// </summary>
		/// <param name="name">milestone alias</param>
		/// <returns></returns>
		internal static MilestoneInfo SearchMilestoneData(string name)
		{
			var searchName = MilestonesData.Keys.FirstOrDefault(key => key.Contains(name, StringComparison.InvariantCultureIgnoreCase));
			if (searchName != null)
			{
				MilestonesData.TryGetValue(searchName, out var milestone);
				return milestone;
			}

			return null;
		}
	}
}
