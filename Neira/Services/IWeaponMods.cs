using Destiny2;
using Destiny2.Definitions;
using Destiny2.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neira.Services
{
	public interface IWeaponMods
	{
		Task<IEnumerable<DestinyInventoryItemDefinition>> GetModsFromManifest();
		Task<IEnumerable<DestinyInventoryItemDefinition>> GetModsFromInventory(DestinyProfileResponse inventory);
	}
}
