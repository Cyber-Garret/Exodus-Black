using System;
using System.Diagnostics;

using AutoMapper;

using Ex077.Entities.Options;
using Ex077.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Ex077.Controllers
{
	public class HomeController : Controller
	{
		private readonly IOptions<BotOptions> _botOptions;
		private readonly IOptions<HomeOptions> _homeOptions;
		private readonly IMapper _mapper;
		public HomeController(IOptions<BotOptions> botOptions, IOptions<HomeOptions> homeOptions, IMapper mapper)
		{
			_botOptions = botOptions;
			_homeOptions = homeOptions;
			_mapper = mapper;
		}

		#region GET

		public IActionResult Index()
		{
			var model = _mapper.Map<HomeViewModel>(_homeOptions.Value);

			return View(model);
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
			RedirectPermanent(_botOptions.Value.ExodusInviteLink);

		[Route("InviteBot")]
		public IActionResult InviteBot() =>
			RedirectPermanent(_botOptions.Value.BotInviteLink);
		#endregion
	}
}
