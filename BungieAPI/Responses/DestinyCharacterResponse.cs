using BungieAPI.Entities;
using BungieAPI.Entities.Characters;

namespace BungieAPI.Responses
{
	public class DestinyCharacterResponse
	{
		public SingleComponentResponse<DestinyInventoryComponent> Inventory { get; set; } = null;
		public SingleComponentResponse<DestinyCharacterComponent> Character { get; set; } = null;
		public SingleComponentResponse<DestinyInventoryComponent> Equipment { get; set; } = null;
		public DestinyItemComponentSetOfInt64 ItemComponents { get; set; } = null;
		public SingleComponentResponse<DestinyCharacterProgressionComponent> Progressions { get; set; }
	}
}