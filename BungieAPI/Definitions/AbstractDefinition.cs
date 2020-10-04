using BungieAPI.Definitions.Common;

using Newtonsoft.Json;

namespace BungieAPI.Definitions
{
	public class AbstractDefinition
	{
		[JsonProperty(PropertyName = "displayProperties")]
		public DestinyDisplayPropertiesDefinition DisplayProperties { get; set; }

		[JsonProperty(PropertyName = "hash")]
		public uint Hash { get; set; }

		[JsonProperty(PropertyName = "redacted")]
		public bool IsRedacted { get; set; }

		public override string ToString()
		{
			return DisplayProperties.Name;
		}
	}
}