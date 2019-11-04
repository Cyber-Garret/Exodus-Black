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

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
