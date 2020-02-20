using Bot.Models;

using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Bot.Core.Data
{
	internal static class ExoticData
	{
		private static readonly ConcurrentDictionary<string, Exotic> ExoticCollection;

		static ExoticData()
		{
			var result = DataStorage.LoadJSONFromHDD<Exotic>(DataStorage.ExoticFolder);

			if (result != null)
				ExoticCollection = result.ToConcurrentDictionary(k => k.Name);
			else
				ExoticCollection = new ConcurrentDictionary<string, Exotic>();
		}
		/// <summary>
		/// Search exotic by full or partial name and return class if founded or return null if not found.
		/// </summary>
		/// <param name="name">Exotic gear name.(Allow full and partial)</param>
		/// <returns>Class or Null</returns>
		internal static Exotic SearchExotic(string name)
		{
			var searchName = ExoticCollection.Keys.FirstOrDefault(key => key.Contains(Alias(name), StringComparison.InvariantCultureIgnoreCase));
			if (searchName != null)
			{
				ExoticCollection.TryGetValue(searchName, out Exotic exotic);
				return exotic;
			}
			else
			{
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
