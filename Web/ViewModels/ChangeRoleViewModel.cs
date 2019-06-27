using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Web.Models;

namespace Web.ViewModels
{
	public class ChangeRoleViewModel
	{
		public string UserId { get; set; }
		public string UserName { get; set; }
		public string UserEmail { get; set; }
		public List<NeiraRole> AllRoles { get; set; }
		public IList<string> UserRoles { get; set; }
		public ChangeRoleViewModel()
		{
			AllRoles = new List<NeiraRole>();
			UserRoles = new List<string>();
		}
	}
}
