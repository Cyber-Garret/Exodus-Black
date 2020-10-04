using BungieAPI.Components.Collectibles;
using BungieAPI.Components.Profiles;
using BungieAPI.Entities;
using BungieAPI.Entities.Characters;

namespace BungieAPI.Responses
{
	public class DestinyProfileResponse
	{
		public SingleComponentResponse<DestinyProfileComponent> Profile { get; set; } = null;
		public SingleComponentResponse<DestinyInventoryComponent> ProfileInventory { get; set; } = null;
		public DictionaryComponentResponseOfInt64<DestinyCharacterComponent> Characters { get; set; } = null;
		public DictionaryComponentResponseOfInt64<DestinyInventoryComponent> CharacterInventories { get; set; } = null;
		public DictionaryComponentResponseOfInt64<DestinyInventoryComponent> CharacterEquipment { get; set; } = null;
		public DictionaryComponentResponseOfInt64<DestinyCharacterProgressionComponent> CharacterProgressions { get; set; } = null;
		public DestinyItemComponentSetOfInt64 ItemComponents { get; set; } = null;
		public SingleComponentResponse<DestinyInventoryComponent> ProfileCurrencies { get; set; }
		public SingleComponentResponse<DestinyProfileProgressionComponent> ProfileProgression { get; set; }
		public SingleComponentResponse<DestinyProfileCollectiblesComponent> ProfileCollectibles { get; set; }
	}
}