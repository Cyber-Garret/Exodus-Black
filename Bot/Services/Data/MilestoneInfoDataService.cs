using Bot.Core.Data;
using Bot.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bot.Services.Data
{
	public class MilestoneInfoDataService : PathConstants, IDataService
	{
		private static readonly ConcurrentDictionary<string, MilestoneInfo> MilestonesData = new ConcurrentDictionary<string, MilestoneInfo>();

		private readonly ILogger logger;
		private readonly string RootDirectory;
		public MilestoneInfoDataService(ILogger<MilestoneInfoDataService> logger, IHostEnvironment env)
		{
			this.logger = logger;
			RootDirectory = env.ContentRootPath;
		}

		public void LoadData()
		{
			var milestoneInfoFolder = Directory.CreateDirectory(Path.Combine(RootDirectory, RootFolder, MilestonesInfoFolder));
			var files = milestoneInfoFolder.GetFiles("*.json");
			if (files.Length > 0)
			{
				foreach (var file in files)
				{
					var milestone = DataStorage.RestoreObject<MilestoneInfo>(file.FullName);
					MilestonesData.TryAdd(milestone.Alias, milestone);
				}
			}

			logger.LogInformation($"Загружено {MilestonesData.Count} данных о этапах.");
		}
		public List<MilestoneInfo> GetAllRaids()
		{
			return MilestonesData.Values.Where(m => m.MilestoneType == MilestoneType.Raid).ToList();
		}

		public List<MilestoneInfo> GetAllStrikes()
		{
			return MilestonesData.Values.Where(m => m.MilestoneType == MilestoneType.Strike).ToList();
		}

		public List<MilestoneInfo> GetAllOther()
		{
			return MilestonesData.Values.Where(m => m.MilestoneType == MilestoneType.Other).ToList();
		}
		public MilestoneInfo SearchMilestoneData(string name)
		{
			var searchName = MilestonesData.Keys.FirstOrDefault(key => key.Contains(name, StringComparison.InvariantCultureIgnoreCase));
			if (searchName != null)
			{
				MilestonesData.TryGetValue(searchName, out var milestone);
				return milestone;
			}
			else
			{
				logger.LogInformation($"Не удалось найти этап: ({name}). В хранилище {MilestonesData.Count} данных о этапах.");
				return null;
			}
		}
	}
}
