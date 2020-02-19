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
	public class MilestoneDataService : PathConstants, IDataService
	{
		private static readonly ConcurrentDictionary<ulong, Milestone> ActiveMilestones = new ConcurrentDictionary<ulong, Milestone>();
		private readonly ILogger logger;
		private readonly string RootDirectory;
		public MilestoneDataService(ILogger<MilestoneDataService> logger, IHostEnvironment env)
		{
			this.logger = logger;
			RootDirectory = env.ContentRootPath;
		}

		/// <summary>
		/// load data from local drive to dictionary
		/// </summary>
		public void LoadData()
		{
			var milestoneFolder = Directory.CreateDirectory(Path.Combine(RootDirectory, RootFolder, MilestonesFolder));
			var files = milestoneFolder.GetFiles("*.json");
			if (files.Length > 0)
			{
				foreach (var file in files)
				{
					var milestone = DataStorage.RestoreObject<Milestone>(file.FullName);
					ActiveMilestones.TryAdd(milestone.MessageId, milestone);
				}
			}

			logger.LogInformation($"Загружено {ActiveMilestones.Count} активных этапов.");
		}

		/// <summary>
		/// Get all active milestones ordered by DateExpire for old to new
		/// </summary>
		/// <returns></returns>
		internal List<Milestone> GetAllMilestones()
		{
			return ActiveMilestones.Values
				.OrderBy(r => r.DateExpire)
				.ToList();
		}

		internal Milestone GetMilestone(ulong messageId)
		{
			ActiveMilestones.TryGetValue(messageId, out Milestone milestone);

			return milestone;
		}

		internal void AddMilestone(Milestone milestone)
		{
			ActiveMilestones.TryAdd(milestone.MessageId, milestone);
		}

		internal void RemoveRaid(ulong MessageId)
		{
			//remove from dictionary
			if (ActiveMilestones.TryRemove(MessageId, out Milestone value))
				//remove file
				DataStorage.RemoveObject(Path.Combine(RootDirectory, RootFolder, MilestonesFolder, $"{value.MessageId}.json"));
		}

		internal void SaveMilestones(params ulong[] ids)
		{
			foreach (var id in ids)
			{
				DataStorage.SaveObject(GetMilestone(id), Path.Combine(RootDirectory, RootFolder, MilestonesFolder, $"{id}.json"), useIndentations: true);
			}
		}

		internal void SaveMilestones()
		{
			foreach (var id in ActiveMilestones.Keys)
			{
				SaveMilestones(id);
			}
		}
	}
}
