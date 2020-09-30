namespace Failsafe.Models
{
	public class Exotic
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public string IconUrl { get; set; }
		public string ImageUrl { get; set; }
		public string Description { get; set; }
		public string Perk { get; set; }
		public string PerkDescription { get; set; }
		public string SecondPerk { get; set; }
		public string SecondPerkDescription { get; set; }
		public string DropLocation { get; set; }
		public bool isWeapon { get; set; } = false;
		public bool isHaveCatalyst { get; set; } = false;
	}
}
