using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Data
{
	internal static class DataStorage
	{
		internal static readonly string BackgroundsFolder;
		internal static readonly string CatalystFolder;
		internal static readonly string ExoticFolder;
		internal static readonly string GuildsFolder;
		internal static readonly string MilestonesFolder;
		internal static readonly string MilestonesInfoFolder;

		private const string res = "resources";
		private const string backgrounds = "backgrounds";
		private const string catalyst = "catalysts";
		private const string exotics = "exotics";
		private const string guilds = "guilds";
		private const string milestonesInfo = "info";
		private const string milestones = "milestones";

		static DataStorage()
		{
			var appDirectory = Directory.GetCurrentDirectory();

			BackgroundsFolder = Path.Combine(appDirectory, res, backgrounds);
			CatalystFolder = Path.Combine(appDirectory, res, catalyst);
			ExoticFolder = Path.Combine(appDirectory, res, exotics);
			GuildsFolder = Path.Combine(appDirectory, res, guilds);
			MilestonesFolder = Path.Combine(appDirectory, res, milestones);
			MilestonesInfoFolder = Path.Combine(appDirectory, res, milestones, milestonesInfo);
		}

		internal static List<T> LoadJSONFromHDD<T>(string filesPath)
		{
			var directory = Directory.CreateDirectory(filesPath);
			var files = directory.GetFiles("*.json");
			if (files.Length > 0)
			{
				var bag = new ConcurrentBag<T>();

				Parallel.ForEach(files, file =>
				{
					bag.Add(RestoreObject<T>(file.FullName));
				});
				return bag.ToList();
			}
			return null;
		}

		/// <summary>
		/// Any IEnumerable to ConcurrentDictionary
		/// </summary>
		internal static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>
			(this IEnumerable<TValue> source, Func<TValue, TKey> valueSelector)
		{
			return new ConcurrentDictionary<TKey, TValue>
					   (source.ToDictionary(valueSelector));
		}

		/// <summary>
		/// Create or read file content by full path and return mapped to class json content
		/// </summary>
		/// <typeparam name="T">Model for restoring</typeparam>
		/// <param name="filePath">Full path to file</param>
		/// <returns></returns>
		private static T RestoreObject<T>(string filePath)
		{
			var json = GetOrCreateFileContents(filePath);
			return JsonConvert.DeserializeObject<T>(json);
		}

		internal static void SaveObject(object obj, string filePath, bool useIndentations)
		{
			var formatting = (useIndentations) ? Formatting.Indented : Formatting.None;

			string json = JsonConvert.SerializeObject(obj, formatting);
			File.WriteAllText(filePath, json, Encoding.UTF8);
		}

		internal static void RemoveObject(string filePath)
		{
			if (File.Exists(filePath))
				File.Delete(filePath);
		}

		private static string GetOrCreateFileContents(string filePath)
		{
			if (!File.Exists(filePath))
			{
				File.WriteAllText(filePath, "", Encoding.UTF8);
				return "";
			}
			return File.ReadAllText(filePath, Encoding.UTF8);
		}
	}
}
