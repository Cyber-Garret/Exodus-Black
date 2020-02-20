using Bot.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bot.Core.Data
{
	internal static class CatalystData
	{
		private static readonly ConcurrentDictionary<string, Catalyst> CatalystCollection;

		static CatalystData()
		{
			var result = DataStorage.LoadJSONFromHDD<Catalyst>(DataStorage.CatalystFolder);

			if (result != null)
				CatalystCollection = result.ToConcurrentDictionary(k => k.WeaponName);
			else
				CatalystCollection = new ConcurrentDictionary<string, Catalyst>();
		}
		/// <summary>
		/// Search catalyst by full or partial name and return class if founded or return null if not found.
		/// </summary>
		/// <param name="name">Exotic gear name.(Allow full and partial)</param>
		/// <returns>Class or Null</returns>
		internal static Catalyst SearchCatalyst(string name)
		{
			var searchName = CatalystCollection.Keys.FirstOrDefault(key => key.Contains(Alias(name), StringComparison.InvariantCultureIgnoreCase));
			if (searchName != null)
			{
				CatalystCollection.TryGetValue(searchName, out var catalyst);
				return catalyst;
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
