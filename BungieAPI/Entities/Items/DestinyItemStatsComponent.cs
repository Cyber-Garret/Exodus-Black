using Newtonsoft.Json;

using System.Collections.Generic;

namespace BungieAPI.Entities.Items
{
	public class DestinyItemStatsComponent
	{
		[JsonProperty(PropertyName = "stats")]
		public IDictionary<uint, DestinyStat> Stats { get; set; }
	}
}