using Bot.Models;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace Bot.Services
{
	public class CatalystDataService
	{
		private const string ResourcesFolder = "resources";
		private const string CatalystsFolder = "catalysts";
		private static readonly ConcurrentDictionary<string, Catalyst> CatalystCollection = new ConcurrentDictionary<string, Catalyst>();

		private readonly ILogger logger;
		private readonly string RootDirectory;

		public CatalystDataService(ILogger<CatalystDataService> logger, IHostEnvironment env)
		{
			this.logger = logger;
			RootDirectory = env.ContentRootPath;
		}

		/// <summary>
		/// Load data from local json files to static ConcurrentDictionary for further usage.
		/// </summary>
		internal void LoadData()
		{
			var exoticsFolder = Directory.CreateDirectory(Path.Combine(RootDirectory, ResourcesFolder, CatalystsFolder));
			var files = exoticsFolder.GetFiles("*.json");
			if (files.Length > 0)
			{
				foreach (var file in files)
				{
					var catalyst = DataStorage.RestoreObject<Catalyst>(file.FullName);
					CatalystCollection.TryAdd(catalyst.WeaponName, catalyst);
				}
			}

			logger.LogInformation($"Загружено {CatalystCollection.Count} ед. катализаторов для оружия.");
		}
		/// <summary>
		/// Search catalyst by full or partial name and return class if founded or return null if not found.
		/// </summary>
		/// <param name="name">Exotic gear name.(Allow full and partial)</param>
		/// <returns>Class or Null</returns>
		internal Catalyst SearchCatalyst(string name)
		{
			var searchName = CatalystCollection.Keys.FirstOrDefault(key => key.Contains(Alias(name), StringComparison.InvariantCultureIgnoreCase));
			if (searchName != null)
			{
				CatalystCollection.TryGetValue(searchName, out var catalyst);
				return catalyst;
			}
			else
			{
				logger.LogInformation($"Не удалось найти катализатор: {Alias(name)}\nВ хранилище {CatalystCollection.Count} ед. катализаторов оружия.");
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
