using System.ComponentModel.DataAnnotations;

namespace Fuselage.Models
{
	public class Welcome
	{
		[Key]
		public int Id { get; set; }

		[MaxLength(5), Display(Name = "Locale")]
		public string Locale { get; set; }

		[MaxLength(200), Display(Name = "Welcome message")]
		public string Message { get; set; }
	}
}
