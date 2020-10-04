using Newtonsoft.Json;

namespace BungieAPI.Definitions
{
	public class DestinyInventoryBucketDefinition : AbstractDefinition
	{
		// scope
		[JsonProperty(PropertyName = "category")]
		public BucketCategory Category { get; set; }
		[JsonProperty(PropertyName = "bucketOrder")]
		public int BucketOrder { get; set; }
		// itemCount
		// location
		// hasTransferDestination
		// enabled
		// fifo
	}
}