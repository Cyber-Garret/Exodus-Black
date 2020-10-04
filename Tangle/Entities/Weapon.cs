using BungieAPI.Definitions;

using System.Collections.Generic;
using System.Linq;

using Tangle.Entities.Models;

namespace Tangle.Entities
{
	/// <summary>
	/// Entity for Destiny 2 weapon
	/// </summary>
	public class Weapon : AbstractDestinyObject
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="baseUrl"></param>
		/// <param name="itemDef"></param>
		/// <param name="bucket"></param>
		/// <param name="perks"></param>
		/// <param name="overrideIconUrl"></param>
		public Weapon(string baseUrl, DestinyInventoryItemDefinition itemDef, DestinyInventoryBucketDefinition bucket, IEnumerable<Perk> perks = null, string overrideIconUrl = null)
			: base(itemDef.DisplayProperties.Name, baseUrl + (overrideIconUrl ?? itemDef.DisplayProperties.Icon), itemDef.Hash)
		{
			Slot = new ItemSlot(bucket);
			Perks = perks ?? Enumerable.Empty<Perk>();
		}
		/// <summary>
		/// Invertory slot for this weapon
		/// </summary>
		public ItemSlot Slot { get; }
		/// <summary>
		/// Perks of this weapon
		/// </summary>
		public IEnumerable<Perk> Perks { get; }

		public override string ToString()
		{
			return $"{Name} ({Slot})";
		}
	}
}
