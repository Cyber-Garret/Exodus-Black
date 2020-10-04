using Newtonsoft.Json;

using System.Collections.Generic;

namespace BungieAPI.Definitions
{
	public class DestinyItemStatBlockDefinition
	{
		// disablePrimaryStatDisplay
		// statGroupHash
		[JsonProperty(PropertyName = "stats")]
		public IDictionary<uint, DestinyInventoryItemStatDefinition> Stats { get; set; }
		// hasDisplayableStats
		[JsonProperty(PropertyName = "primaryBaseStatHash")]
		public uint PrimaryBaseStatHash { get; set; }
	}
}