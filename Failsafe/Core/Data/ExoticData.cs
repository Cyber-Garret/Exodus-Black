using System;
using System.Collections.Concurrent;
using System.Linq;

using Failsafe.Models;

namespace Failsafe.Core.Data
{
	internal static class ExoticData
	{
		private static readonly ConcurrentDictionary<string, Exotic> ExoticsData;

		static ExoticData()
		{
			var result = DataStorage.LoadJSONFromHDD<Exotic>(DataStorage.ExoticFolder);

			if (result != null)
				ExoticsData = result.ToConcurrentDictionary(k => k.Name);
			else
				ExoticsData = new ConcurrentDictionary<string, Exotic>();
		}

		internal static Exotic SearchExotic(string name)
		{
			var searchName = ExoticsData.Keys.FirstOrDefault(key => key.Contains(Alias(name), StringComparison.InvariantCultureIgnoreCase));
			if (searchName != null)
			{
				ExoticsData.TryGetValue(searchName, out var exotic);

				return exotic;
			}
			else
				return null;
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
