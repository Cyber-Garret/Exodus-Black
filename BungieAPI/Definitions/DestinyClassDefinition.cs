using Newtonsoft.Json;

namespace BungieAPI.Definitions
{
    public class DestinyClassDefinition : AbstractDefinition
    {
        [JsonProperty(PropertyName = "classType")]
        public DestinyClass ClassType { get; set; }
    }
}