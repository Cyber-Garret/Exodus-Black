using System.ComponentModel.DataAnnotations;

namespace Neira.Bot.Models.Db
{
	public class Catalyst
	{
		[Key]
		public int Id { get; set; }
		[Required, MaxLength(100)]
		public string WeaponName { get; set; }
		[Required, MaxLength(1000)]
		public string Icon { get; set; }
		[Required, MaxLength(2000)]
		public string Description { get; set; }
		[Required, MaxLength(1000)]
		public string DropLocation { get; set; }
		[Required, MaxLength(1000)]
		public string Masterwork { get; set; }
		[Required, MaxLength(1000)]
		public string Bonus { get; set; }
	}
}
