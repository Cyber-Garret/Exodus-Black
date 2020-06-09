using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Web.Controllers
{
	[Authorize]
	public class AdminController : Controller
	{
		private readonly IStringLocalizer<HomeController> _localizer;
		public AdminController(IStringLocalizer<HomeController> localizer)
		{
			_localizer = localizer;
		}

		public IActionResult Index()
		{
			ViewData["Welcome"] = _localizer["Welcome"];
			return View();
		}
	}
}
