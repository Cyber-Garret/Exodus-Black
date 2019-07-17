using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using Web.ViewModels;

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
		[Route("Privacy")]
		public IActionResult Privacy()
		{
			return View();
		}
		[Route("About")]
		public IActionResult About()
		{
			return View();
		}
		[Route("Donate")]
		public IActionResult Donate()
		{
			return View();
		}
		[Route("Contact")]
		public IActionResult Contact()
		{
			return View();
		}
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		[Route("AddMe")]
		public IActionResult Add_Me()
		{
			return RedirectPermanent(@"https://discordapp.com/oauth2/authorize?client_id=521693707238506498&scope=bot&permissions=8");
		}
		[Route("JoinMe")]
		public IActionResult Join_Me()
		{
			return RedirectPermanent(@"https://discord.gg/WcuNPM9");
		}

		[Route("DevelopmentBoard")]
		public IActionResult Trello()
		{
			return RedirectPermanent(@"https://trello.com/b/HhOiSSn4");
		}
		#endregion
	}
}
