using Destiny2;
using Destiny2.Definitions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Destiny2.Responses;

namespace Neira.Services
{
	public class WeaponMods : IWeaponMods
	{
		private readonly IManifest _manifest;
		private readonly ILogger _logger;

		//weapon mods have 59 and 1052191496 and 610365472
		private const uint WeaponModsDamageCategoryHash = 1052191496;
		private const uint DummiesCategoryHash = 3109687656;

		public WeaponMods(IServiceProvider service)
		{
			_manifest = service.GetRequiredService<IManifest>();
			_logger = service.GetRequiredService<ILogger<WeaponMods>>();
		}

		public async Task<IEnumerable<DestinyInventoryItemDefinition>> GetModsFromManifest()
		{
			var manifestMods = await _manifest.LoadInventoryItemsWithCategory(WeaponModsDamageCategoryHash);
			var filteredMods = manifestMods.Where(mod =>
				!mod.ItemCategoryHashes.Contains(DummiesCategoryHash) && // some mods for weapon damage have this category (?)
				!mod.DisplayProperties.Description.Contains("Этот модификатор больше не действителен")); // year 1 mods

			return filteredMods;
		}

		public async Task<IEnumerable<DestinyInventoryItemDefinition>> GetModsFromInventory(DestinyProfileResponse inventory)
		{
			var mods = new List<DestinyInventoryItemDefinition>();

			foreach (var item in inventory.CharacterCurrencyLookups.Data)
			{
				foreach (var keys in item.Value.ItemQuantities.Keys)
				{
					var itemDef = await _manifest.LoadInventoryItem(keys);
					if (itemDef.ItemCategoryHashes.Contains(WeaponModsDamageCategoryHash))
					{
						mods.Add(itemDef);
					}
				}
			}

			return mods;
		}
	}
}
