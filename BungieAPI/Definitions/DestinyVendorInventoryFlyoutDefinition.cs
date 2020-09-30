using System.Collections.Generic;
using BungieAPI.Definitions.Common;

namespace BungieAPI.Definitions
{
    public class DestinyVendorInventoryFlyoutDefinition
    {
        public string LockedDescription { get; set; }
        public DestinyDisplayPropertiesDefinition DisplayProperties { get; set; }
        public IEnumerable<DestinyVendorInventoryFlyoutBucketDefinition> Buckets { get; set; }
        public uint FlyoutId { get; set; }
        public bool SuppressNewness { get; set; }
        public uint? EquipmentSlotHash { get; set; }
    }
}