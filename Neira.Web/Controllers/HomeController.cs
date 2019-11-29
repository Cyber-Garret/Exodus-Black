using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Neira.Database;
using Neira.Models;
using Neira.ViewModels;

using System.Diagnostics;
using System.Linq;

namespace Neira.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public IActionResult Index()
		{
			using var Db = new NeiraLinkContext();
			var model = new IndexViewModel
			{
				BotInfo = Db.BotInfos.FirstOrDefault()
			};

			return View(model);
		}

		[Route("AddBot")]
		public IActionResult AddBot() => RedirectPermanent($"https://discordapp.com/oauth2/authorize?client_id=521693707238506498&scope=bot&permissions=269479104");

		[Route("YandexMoney")]
		public IActionResult YandexMoney() => RedirectPermanent($"https://money.yandex.ru/to/410019748161790");

		[Route("Patreon")]
		public IActionResult Patreon() => RedirectPermanent($"https://www.patreon.com/Cyber_Garret");

		[Route("BlackExodus")]
		public IActionResult BlackExodus() => RedirectPermanent($"https://discord.gg/WcuNPM9");

		[Route("VKGroup")]
		public IActionResult VkGroup() => RedirectPermanent($"https://vk.com/failsafe_bot");

		[Route("MyGithub")]
		public IActionResult Github() => RedirectPermanent($"https://github.com/Cyber-Garret");

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
