using System.Collections.Generic;

namespace BungieAPI.Definitions
{
	public class DestinyItemSocketCategoryDefinition
	{
		public uint SocketCategoryHash { get; set; }
		public IEnumerable<int> SocketIndexes { get; set; }
	}
}