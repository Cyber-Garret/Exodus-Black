using System.ComponentModel.DataAnnotations;

namespace Neiralink.Models
{
	public class RandomWelcome
	{
		[Key]
		public int RowID { get; set; }
		[MaxLength(200), Display(Name = "English locale")]
		public string EN { get; set; }
		[MaxLength(200), Display(Name = "Russian locale")]
		public string RU { get; set; }
		[MaxLength(200), Display(Name = "Ukrainian locale")]
		public string UK { get; set; }
	}
}
