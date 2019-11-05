using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Neira.MVC.Models;
using Neira.MVC.Models.NeiraLink;
using Neira.MVC.ViewModels;

namespace Neira.MVC.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly NeiraContext neira;

		public HomeController(ILogger<HomeController> logger, NeiraContext neiraContext)
		{
			_logger = logger;
			neira = neiraContext;
		}

		public IActionResult Index()
		{
			var model = new IndexViewModel
			{
				BotInfo = neira.BotInfos.FirstOrDefault()
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
