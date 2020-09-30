using System.Collections.Generic;

namespace BungieAPI.Entities.Vendors
{
    public class DestinyVendorCategory
    {
        public int DisplayCategoryIndex { get; set; }
        public IEnumerable<int> ItemIndexes { get; set; }
    }
}