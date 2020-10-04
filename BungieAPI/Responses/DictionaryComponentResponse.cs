using System.Collections.Generic;

namespace BungieAPI.Responses
{
	public class DictionaryComponentResponse<TKey, TValue>
	{
		public IDictionary<TKey, TValue> Data { get; set; }
	}
}