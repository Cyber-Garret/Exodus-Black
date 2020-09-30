using System.Collections.Generic;

namespace BungieAPI.Entities.Characters
{
    public class DestinyCharacterProgressionComponent
    {
        public IDictionary<uint, DestinyProgression> Progressions { get; set; }
    }
}