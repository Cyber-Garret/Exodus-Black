using System.Collections.Generic;

namespace BungieAPI.Definitions
{
    public class DestinyProgressionStepDefinition
    {
        public string StepName { get; set; }
        public DestinyProgressionStepDisplayEffect DisplayEffectType { get; set; }
        public int ProgressTotal { get; set; }
        public IEnumerable<DestinyItemQuantity> RewardItems { get; set; }
        public string Icon { get; set; }
    }
}