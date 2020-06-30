using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using WebSite.Models;
using WebSite.ViewModels;

namespace WebSite.Controllers
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

		[Route("Login"), HttpGet]
		public IActionResult Login()
		{
			return View();
		}

		[Route("Login"), HttpPost]
		public async Task<IActionResult> LoginAsync(LoginViewModel credential)
		{
			if (ModelState.IsValid)
			{
				var user = _configuration.GetSection("SiteAdmin").Get<SiteAdmin>();

				if (credential.UserName == user.UserName)
				{
					var passwordHasher = new PasswordHasher<string>();
					if (passwordHasher.VerifyHashedPassword(null, user.Password, credential.Password) == PasswordVerificationResult.Success)
					{
						await Authenticate(user);

						return RedirectToAction(actionName: "Index", controllerName: "Admin");
					}
				}
				ViewData["Message"] = "Invalid attempt";
			}

			return View();
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
