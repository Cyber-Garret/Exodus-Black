using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Destiny2;
using Destiny2.Definitions;
using Destiny2.Entities.Items;

using Microsoft.AspNetCore.Mvc;

using WebSite.Models;
using WebSite.ViewModels;

namespace WebSite.Controllers
{
	public class GodRollController : Controller
	{
		private const uint Kinetic = 1498876634;
		private readonly IManifest manifest;
		public GodRollController(IManifest manifest)
		{
			this.manifest = manifest;
		}
		public async Task<IActionResult> Index()
		{
			var weapons = GetItems(await manifest.LoadInventoryItemsWithCategory(1)).OrderBy(o => o.Name);

			var model = new GodRollViewModel
			{
				Weapons = weapons.Take(100)
			};
			return View(model);
		}

		private IEnumerable<Item> GetItems(IEnumerable<DestinyInventoryItemDefinition> inventoryItemDefinition)
		{

			foreach (var itemDefinition in inventoryItemDefinition)
			{
				var bucket = manifest.LoadBucket(itemDefinition.Inventory.BucketTypeHash).Result;

				if (itemDefinition.Sockets != null)
					yield return new Item("https://www.bungie.net", itemDefinition, bucket, LoadPerks(itemDefinition.Sockets.SocketEntries).Result);
			}
		}

		public async Task<IEnumerable<Perk>> LoadPerks(IEnumerable<DestinyItemSocketEntryDefinition> socketBlockDefinition = null)
		{
			if (socketBlockDefinition.Count() < 1)
				return new List<Perk>();

			List<Perk> perks = new List<Perk>();
			foreach (var socket in socketBlockDefinition)
			{
				if (socket.SingleInitialItemHash == 0)
					continue;
				var plug = await manifest.LoadInventoryItem(socket.SingleInitialItemHash);
				var categories = await manifest.LoadItemCategories(plug.ItemCategoryHashes);
				perks.Add(new Perk("https://www.bungie.net", plug, categories));
			}
			return perks;
		}

		public async Task<Perk> LoadPerk(uint hash)
		{
			var plug = await manifest.LoadInventoryItem(hash);
			var categories = await manifest.LoadItemCategories(plug.ItemCategoryHashes);
			return new Perk("https://www.bungie.net", plug, categories);
		}
	}
}
