﻿using System.ComponentModel.DataAnnotations;

namespace Ex077.ViewModels
{
	public class LoginViewModel
	{
		[Required(ErrorMessage = "Required Login")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "Required Password")]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}
