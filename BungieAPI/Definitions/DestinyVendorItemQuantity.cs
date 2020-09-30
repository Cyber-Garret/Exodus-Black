namespace BungieAPI.Definitions
{
    public class DestinyVendorItemQuantity
    {
        public uint ItemHash { get; set; }
        public long? ItemInstanceId { get; set; }
        public int Quantity { get; set; }
    }
}