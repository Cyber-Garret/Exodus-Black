using Newtonsoft.Json;

namespace BungieAPI.Definitions
{
	public class DestinyInventoryItemStatDefinition
	{
		[JsonProperty(PropertyName = "statHash")]
		public uint StatHash { get; set; }
		[JsonProperty(PropertyName = "value")]
		public int Value { get; set; }
		[JsonProperty(PropertyName = "minimum")]
		public int Minimum { get; set; }
		[JsonProperty(PropertyName = "maximum")]
		public int Maximum { get; set; }
	}
}