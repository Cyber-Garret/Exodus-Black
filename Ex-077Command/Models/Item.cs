using Destiny2;
using Destiny2.Definitions;
using Destiny2.Entities.Items;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSite.Models
{
	public class Item
	{
		public Item(string baseUrl, DestinyInventoryItemDefinition itemDef, DestinyInventoryBucketDefinition bucket, IEnumerable<Perk> perks = null, string overrideIconUrl = null)
		{
			Hash = itemDef.Hash;
			Name = itemDef.DisplayProperties.Name;
			Slot = new ItemSlot(bucket);
			Perks = perks ?? Enumerable.Empty<Perk>();
			Icon = baseUrl + (overrideIconUrl ?? itemDef.DisplayProperties.Icon);
		}

		public uint Hash { get; }
		public string Icon { get; }
		public string Name { get; }
		public ItemSlot Slot { get; }
		public IEnumerable<Perk> Perks { get; }

		public override string ToString()
		{
			return $"{Name} ({Slot})";
		}
	}
}
