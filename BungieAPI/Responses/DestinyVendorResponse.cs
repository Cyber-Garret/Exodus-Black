using BungieAPI.Components.Inventory;
using BungieAPI.Entities.Vendors;

namespace BungieAPI.Responses
{
    public class DestinyVendorResponse
    {
        public SingleComponentResponse<DestinyVendorComponent> Vendor { get; set; }
        public SingleComponentResponse<DestinyVendorCategoriesComponent> Categories { get; set; }
        public DictionaryComponentResponse<int, DestinyVendorSaleItemComponent> Sales { get; set; }
        // itemComponents
        public SingleComponentResponse<DestinyCurrenciesComponent> CurrencyLookups { get; set; }
    }
}