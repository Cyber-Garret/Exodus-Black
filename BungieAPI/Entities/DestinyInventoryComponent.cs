using BungieAPI.Entities.Items;

using Newtonsoft.Json;

using System.Collections.Generic;

namespace BungieAPI.Entities
{
	public class DestinyInventoryComponent
	{
		[JsonProperty(PropertyName = "items")]
		public IEnumerable<DestinyItemComponent> Items { get; set; }
	}
}