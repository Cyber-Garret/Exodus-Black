using Destiny2;

using Neira.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neira.Services
{
    public interface IMaxPowerService
    {
        Task<IDictionary<ItemSlot.SlotHashes, Item>> ComputeMaxPowerAsync(BungieMembershipType type, long accountId, long characterId);
        decimal ComputePower(IEnumerable<Item> items);
    }
}
