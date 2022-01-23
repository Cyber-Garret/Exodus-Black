using System;
using System.Collections.Concurrent;
using System.Linq;

using Failsafe.Models;

namespace Failsafe.Core.Data
{
	internal static class CatalystData
	{
		private static readonly ConcurrentDictionary<string, Catalyst> Data;

		static CatalystData()
		{
			var result = DataStorage.LoadJSONFromHDD<Catalyst>(DataStorage.CatalystFolder);

			if (result != null)
				Data = result.ToConcurrentDictionary(k => k.WeaponName);
			else
				Data = new ConcurrentDictionary<string, Catalyst>();
		}

		internal static Catalyst Search(string name)
		{
			var searchName = Data.Keys.FirstOrDefault(key => key.Contains(Alias(name), StringComparison.InvariantCultureIgnoreCase));
			if (searchName != null)
			{
				Data.TryGetValue(searchName, out var catalyst);

				return catalyst;
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
				_ => name,
			};
		}
	}
}
