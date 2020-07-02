using Dapper.Contrib.Extensions;

namespace Neiralink.Models
{
	public class Catalyst
	{
		[Key]
		public int RowID { get; set; }
		public LangKey Lang { get; set; }
		public string WeaponName { get; set; }
		public string IconUrl { get; set; }
		public string Description { get; set; }
		public string DropLocation { get; set; }
		public string Masterwork { get; set; }
		public string Bonus { get; set; }
	}
}
