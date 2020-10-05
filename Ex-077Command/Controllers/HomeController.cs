using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics;

using Ex077.ViewModels;

namespace Ex077.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		#region GET
		public IActionResult Index()
		{
			return View(new HomeViewModel());
		}
		[Route("Donate")]
		public IActionResult Donate()
		{
			return View(new DonateViewModel());
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
		#endregion

		#region POST
		[HttpPost]
		public IActionResult SetLanguage(string culture, string returnUrl)
		{
			Response.Cookies.Append(
				CookieRequestCultureProvider.DefaultCookieName,
				CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
				new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
			);

			return LocalRedirect(returnUrl);
		}
		#endregion

		#region Routes
		[Route("AddBot")]
		public IActionResult AddBot() => RedirectPermanent($"https://discord.com/oauth2/authorize?client_id=521693707238506498&scope=bot&permissions=269479104");

		[Route("YandexMoney")]
		public IActionResult YandexMoney() => RedirectPermanent($"https://money.yandex.ru/to/410019748161790");

		[Route("BlackExodus")]
		public IActionResult BlackExodus() => RedirectPermanent($"https://discord.com/invite/WcuNPM9");
		#endregion
	}
}
