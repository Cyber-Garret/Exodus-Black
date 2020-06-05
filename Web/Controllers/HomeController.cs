using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using System.Diagnostics;

using Web.Models;

namespace Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly IStringLocalizer<HomeController> _localizer;
		private readonly ILogger<HomeController> _logger;

		public HomeController(IStringLocalizer<HomeController> localizer, ILogger<HomeController> logger)
		{
			_localizer = localizer;
			_logger = logger;
		}

		public IActionResult Index()
		{
			ViewData["Welcome"] = _localizer["Welcome"];
			_logger.LogInformation(_localizer["Welcome"]);
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
