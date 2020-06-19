using System.ComponentModel.DataAnnotations;

namespace Neiralink.Models
{
	public struct RandomWelcome
	{
		[Key]
		public int RowID { get; set; }
		[MaxLength(200)]
		public string EN { get; set; }
		[MaxLength(200)]
		public string RU { get; set; }
		[MaxLength(200)]
		public string UK { get; set; }
	}
}
