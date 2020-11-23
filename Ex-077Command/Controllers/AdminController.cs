using Ex077.Models;
using Ex077.ViewModels;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ex077.Controllers
{
	public class AdminController : Controller
	{
		private readonly IConfiguration _configuration;

		public AdminController(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Index(LoginViewModel credential)
		{
			if (ModelState.IsValid)
			{
				var user = _configuration.GetSection("SiteAdmins").Get<List<SiteAdmin>>().FirstOrDefault(u => u.UserName == credential.UserName);
				if (user != null)
				{
					var passwordHasher = new PasswordHasher<string>();
					if (passwordHasher.VerifyHashedPassword(null, user.Password, credential.Password) == PasswordVerificationResult.Success)
					{
						await Authenticate(user);

						return RedirectToAction(actionName: "Index", controllerName: "Admin");
					}
				}
				ModelState.AddModelError("", "Invalid attempt");
			}
			return View(credential);
		}

		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction("Index", "Admin");
		}

		[NonAction]
		private async Task Authenticate(SiteAdmin credential)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, credential.UserName)
			};

			var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
		}
	}
}
