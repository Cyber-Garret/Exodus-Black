using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using Web.Models;

using Core;

namespace Web.Controllers
{
	public class HomeController : Controller
	{
		readonly FailsafeContext db;
		public HomeController(FailsafeContext context)
		{
			db = context;
		}
		#region Actions
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		public IActionResult About()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
		#endregion
	}
}
