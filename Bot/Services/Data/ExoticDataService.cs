using Bot.Core.Data;
using Bot.Models;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace Bot.Services.Data
{
	public class ExoticDataService : PathConstants
	{
		private static readonly ConcurrentDictionary<string, Exotic> ExoticCollection = new ConcurrentDictionary<string, Exotic>();

		private readonly ILogger logger;
		private readonly string RootDirectory;

		public ExoticDataService(ILogger<ExoticDataService> logger, IHostEnvironment env)
		{
			this.logger = logger;
			RootDirectory = env.ContentRootPath;
		}

		/// <summary>
		/// Load data from local json files to static ConcurrentDictionary for further usage.
		/// </summary>
		internal void LoadData()
		{
			
			var exoticsFolder = Directory.CreateDirectory(Path.Combine(RootDirectory, RootFolder, ExoticsFolder));
			var files = exoticsFolder.GetFiles("*.json");
			if (files.Length > 0)
			{
				foreach (var file in files)
				{
					var exotic = DataStorage.RestoreObject<Exotic>(file.FullName);
					ExoticCollection.TryAdd(exotic.Name, exotic);
				}
			}

			logger.LogInformation($"Загружено {ExoticCollection.Count} ед. экзотического снаряжения.");
		}
		/// <summary>
		/// Search exotic by full or partial name and return class if founded or return null if not found.
		/// </summary>
		/// <param name="name">Exotic gear name.(Allow full and partial)</param>
		/// <returns>Class or Null</returns>
		internal Exotic SearchExotic(string name)
		{
			var searchName = ExoticCollection.Keys.FirstOrDefault(key => key.Contains(Alias(name), StringComparison.InvariantCultureIgnoreCase));
			if (searchName != null)
			{
				ExoticCollection.TryGetValue(searchName, out Exotic exotic);
				return exotic;
			}
			else
			{
				logger.LogInformation($"Не найдено экзотическое снаряжение: ({Alias(name)}). В хранилище {ExoticCollection.Count} ед. экзотического снаряжения.");
				return null;
			}
		}

		private static string Alias(string name)
		{
			return name switch
			{
				"дарси" => "Д.А.Р.С.И.",
				"мида" => "MIDA",
				"сурос" => "SUROS",
				"морозники" => "M0р03ники",
				"топотуны" => "Т0п0тунЬI",
				_ => name,
			};
		}

	}
}
