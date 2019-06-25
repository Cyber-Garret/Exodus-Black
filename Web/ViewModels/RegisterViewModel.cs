using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels
{
	public class RegisterViewModel
	{
		[Required]
		[Display(Name = "Имя")]
		public string DisplayName { get; set; }

		[Required]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Required]
		[Range(1912, 2077), Display(Name = "Год рождения")]
		public int Year { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Пароль")]
		public string Password { get; set; }

		[Required]
		[Compare("Password", ErrorMessage = "Пароли не совпадают")]
		[DataType(DataType.Password)]
		[Display(Name = "Подтвердить пароль")]
		public string PasswordConfirm { get; set; }
	}
}
