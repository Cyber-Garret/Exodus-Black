using Newtonsoft.Json;

namespace BungieAPI.Definitions
{
    public class DestinyItemInventoryBlockDefinition
    {
        // stackUniqueLabel
        // maxStackSize
        [JsonProperty(PropertyName = "bucketTypeHash")]
        public uint BucketTypeHash { get; set; }
        // recoveryBucketTypeHash
        // recoveryBucketTypeHash
        [JsonProperty(PropertyName = "isInstanceItem")]
        public bool IsInstanceItem { get; set; }
        // tierTypeName
        [JsonProperty(PropertyName = "tierType")]
        public TierType TierType { get; set; }
        // expirationTooltip
        // expiredInActivityMessage
        // expiredInOrbitMessage
        // suppressExpirationWhenObjectivesComplete
    }
}