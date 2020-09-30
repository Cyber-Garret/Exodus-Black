using Newtonsoft.Json;

namespace BungieAPI.Responses
{
    public class SingleComponentResponse<T>
    {
        [JsonProperty(PropertyName = "data")]
        public T Data { get; set; }
    }
}