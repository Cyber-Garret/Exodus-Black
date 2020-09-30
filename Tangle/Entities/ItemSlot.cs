using BungieAPI.Definitions;

namespace Tangle.Entities
{

	/// <summary>
	/// Entity for Destiny 2 invertory item slot
	/// </summary>
	public class ItemSlot
	{
		public ItemSlot(DestinyInventoryBucketDefinition bucket)
		{
			Name = bucket.DisplayProperties.Name;
			Order = bucket.BucketOrder;
			Hash = (SlotHashes)bucket.Hash;
		}

		public ItemSlot(string name, SlotHashes hash, int order = 0)
		{
			Name = name;
			Order = order;
			Hash = hash;
		}

		public string Name { get; }
		public int Order { get; }
		public SlotHashes Hash { get; }

		public bool IsWeapon
		{
			get
			{
				return Hash switch
				{
					SlotHashes.Kinetic => true,
					SlotHashes.Energy => true,
					SlotHashes.Power => true,
					_ => false,
				};
			}
		}

		public bool IsArmor
		{
			get
			{
				return Hash switch
				{
					SlotHashes.Helmet => true,
					SlotHashes.Gauntlet => true,
					SlotHashes.ChestArmor => true,
					SlotHashes.LegArmor => true,
					SlotHashes.ClassArmor => true,
					_ => false,
				};
			}
		}

		public enum SlotHashes : uint
		{
			Kinetic = 1498876634,
			Energy = 2465295065,
			Power = 953998645,

			Helmet = 3448274439,
			Gauntlet = 3551918588,
			ChestArmor = 14239492,
			LegArmor = 20886954,
			ClassArmor = 1585787867,

			Ghost = 4023194814,
			Vehicle = 2025709351,
			Ships = 284967655,
			Subclass = 3284755031,
			ClassBanners = 4292445962,
			Emblems = 4274335291,
			Emotes = 1107761855,
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ItemSlot slot))
			{
				return false;
			}

			return Hash == slot.Hash;
		}

		public override int GetHashCode()
		{
			return (int)Hash;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
