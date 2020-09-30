using BungieAPI.Definitions;

using System.Collections.Generic;
using System.Linq;

using Tangle.Entities.Models;

namespace Tangle.Entities
{
	/// <summary>
	/// Entity for Destiny 2 weapon perk
	/// </summary>
	public class Perk : AbstractDestinyObject
	{
		public Perk(string baseUrl, DestinyInventoryItemDefinition plug, IEnumerable<DestinyItemCategoryDefinition> categories)
			: base(baseUrl, plug)
		{
			Description = plug.DisplayProperties.Description;
			CategoryHash = plug.Plug.PlugCategoryHash;
			Categories = categories;
		}

		private readonly ISet<uint> _modCategories = new HashSet<uint>
		{
			4104513227, // Armor Mods
            56,         // Solstice Armor Glows (I hope)
        };
		public uint CategoryHash { get; }
		public string Description { get; }
		public IEnumerable<DestinyItemCategoryDefinition> Categories { get; }

		// TODO: Might need to update this for weapon mods if that is ever needed.
		public bool IsMod
		{
			get
			{
				var categoryHashes = Categories.Select(category => category.Hash);
				var intersection = _modCategories.Intersect(categoryHashes);
				return intersection.Any();
			}
		}
	}
}
