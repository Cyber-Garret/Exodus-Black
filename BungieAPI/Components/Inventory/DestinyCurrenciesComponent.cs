using System.Collections.Generic;

namespace BungieAPI.Components.Inventory
{
    public class DestinyCurrenciesComponent
    {
        public IDictionary<uint, int> ItemQuantities { get; set; }
    }
}