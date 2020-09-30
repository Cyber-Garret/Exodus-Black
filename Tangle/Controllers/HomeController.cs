using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using BungieAPI;
using BungieAPI.Config;
using BungieAPI.Definitions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Tangle.Entities;
using Tangle.Entities.ViewModels;

namespace Tangle.Controllers
{
	public class HomeController : Controller
	{
		private readonly IManifest _manifest;

		public HomeController(IManifest manifest)
		{
			_manifest = manifest;
		}

		#region Actions
		public async Task<IActionResult> IndexAsync()
		{
			var weapons = GetWeapons(await _manifest.LoadInventoryItemsWithCategory(1)).OrderBy(o => o.Name);

			var model = new IndexViewModel
			{
				Weapons = weapons.Take(100)
			};
			return View(model);
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
		#endregion

		#region Methods
		private IEnumerable<Weapon> GetWeapons(IEnumerable<DestinyInventoryItemDefinition> inventoryItemDefinition)
		{

			foreach (var itemDefinition in inventoryItemDefinition)
			{
				var bucket = _manifest.LoadBucket(itemDefinition.Inventory.BucketTypeHash).Result;

				if (itemDefinition.Sockets != null)
					yield return new Weapon("https://www.bungie.net", itemDefinition, bucket, LoadPerks(itemDefinition.Sockets.SocketEntries).Result);
			}
		}

		private async Task<IEnumerable<Perk>> LoadPerks(IEnumerable<DestinyItemSocketEntryDefinition> socketBlockDefinition = null)
		{
			if (socketBlockDefinition.Count() < 1)
				return new List<Perk>();

			List<Perk> perks = new List<Perk>();
			foreach (var socket in socketBlockDefinition)
			{
				if (socket.SingleInitialItemHash == 0)
					continue;
				var plug = await _manifest.LoadInventoryItem(socket.SingleInitialItemHash);
				var categories = await _manifest.LoadItemCategories(plug.ItemCategoryHashes);
				perks.Add(new Perk("https://www.bungie.net", plug, categories));
			}
			return perks;
		}

		//public async Task<Perk> LoadPerk(uint hash)
		//{
		//	var plug = await _manifest.LoadInventoryItem(hash);
		//	var categories = await _manifest.LoadItemCategories(plug.ItemCategoryHashes);
		//	return new Perk("https://www.bungie.net", plug, categories);
		//}
		#endregion
	}
}
