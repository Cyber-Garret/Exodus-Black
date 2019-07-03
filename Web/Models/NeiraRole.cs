using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
	public class NeiraRole : IdentityRole
	{
		[Required]
		public string Icon { get; set; }

		[Required]
		public string DisplayName { get; set; }
	}
}
