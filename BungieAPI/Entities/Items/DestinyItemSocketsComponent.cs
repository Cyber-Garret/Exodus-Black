using Newtonsoft.Json;

using System.Collections.Generic;

namespace BungieAPI.Entities.Items
{
	public class DestinyItemSocketsComponent
	{
		[JsonProperty(PropertyName = "sockets")]
		public IEnumerable<DestinyItemSocketState> Sockets { get; set; }
	}
}