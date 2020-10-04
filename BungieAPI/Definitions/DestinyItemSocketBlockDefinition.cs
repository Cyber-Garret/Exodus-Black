using System.Collections.Generic;

namespace BungieAPI.Definitions
{
	public class DestinyItemSocketBlockDefinition
	{
		// details
		public IEnumerable<DestinyItemSocketEntryDefinition> SocketEntries { get; set; }
		public IEnumerable<DestinyItemIntrinsicSocketEntryDefinition> IntrinsicSockets { get; set; }
		public IEnumerable<DestinyItemSocketCategoryDefinition> SocketCategories { get; set; }
	}
}