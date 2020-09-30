using System.Collections.Generic;

namespace BungieAPI.Entities
{
    public class DestinyProfileComponent
    {
        public IEnumerable<long> CharacterIds { get; set; } = new List<long>();
        public IEnumerable<uint> SeasonHashes { get; set; } = new List<uint>();
    }
}