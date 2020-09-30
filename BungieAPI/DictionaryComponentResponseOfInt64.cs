using System.Collections.Generic;
using Newtonsoft.Json;

namespace BungieAPI
{
    public class DictionaryComponentResponseOfInt64<T>
    {
        [JsonProperty(PropertyName = "data")]
        public IDictionary<long, T> Data { get; set; } = null;
        // privacy
    }
}