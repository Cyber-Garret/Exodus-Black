using Ex077.Entities;
using Ex077.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using System;
using System.Diagnostics;

namespace Ex077.Controllers
{
	public class HomeController : Controller
	{
		private readonly IOptions<BotOptions> _options;

		public HomeController(IOptions<BotOptions> options)
		{
			_options = options;
		}

		#region GET
		public IActionResult Index()
		{
			return View(new HomeViewModel());
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

		[Route("BlackExodus")]
		public IActionResult BlackExodus() =>
			RedirectPermanent(_options.Value.ExodusInviteLink);

		[Route("AddEnBot")]
		public IActionResult AddEnBot() =>
			RedirectPermanent(_options.Value.EnBotLink);

		[Route("EnDocs")]
		public IActionResult EnDocs() =>
			RedirectPermanent(_options.Value.EnDocsLink);

		[Route("AddRuBot")]
		public IActionResult AddRuBot() =>
			Redirect(_options.Value.RuBotLink);

		[Route("RuDocs")]
		public IActionResult RuDocs() =>
			RedirectPermanent(_options.Value.RuDocsLink);

		[Route("AddUaBot")]
		public IActionResult AddUaBot() =>
			RedirectPermanent(_options.Value.UaBotLink);

		[Route("UaDocs")]
		public IActionResult UaDocs() =>
			RedirectPermanent(_options.Value.UaDocsLink);

		#endregion
	}
}
