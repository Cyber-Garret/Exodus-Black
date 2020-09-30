using System.Collections.Generic;
using BungieAPI.Entities.Items;
using Newtonsoft.Json;

namespace BungieAPI.Entities
{
    public class DestinyInventoryComponent
    {
        [JsonProperty(PropertyName = "items")]
        public IEnumerable<DestinyItemComponent> Items { get; set; }
    }
}