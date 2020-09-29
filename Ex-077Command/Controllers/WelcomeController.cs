
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Neiralink;
using Neiralink.Models;

namespace WebSite.Controllers
{
	[Authorize]
	public class WelcomeController : Controller
	{
		private readonly IWelcomeDbClient db;
		public WelcomeController(IWelcomeDbClient dbClient)
		{
			db = dbClient;
		}
		// GET: WelcomeController
		public ActionResult Index()
		{
			return View(db.GetAllWelcomes());
		}

		// GET: WelcomeController/Create
		public ActionResult Create()
		{
			return View();
		}

		// POST: WelcomeController/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(RandomWelcome welcome)
		{
			try
			{
				db.CreateWelcome(welcome);
				return RedirectToAction(nameof(Index));
			}
			catch
			{
				return View();
			}
		}

		// GET: WelcomeController/Edit/5
		public ActionResult Edit(int id)
		{
			var welcome = db.GetWelcome(id);
			if (welcome != null)
				return View(welcome);
			return NotFound();
		}

		// POST: WelcomeController/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(RandomWelcome welcome)
		{
			try
			{
				db.UpdateWelcome(welcome);
				return RedirectToAction(nameof(Index));
			}
			catch
			{
				return View();
			}
		}

		// GET: WelcomeController/Delete/5
		[ActionName("Delete")]
		public ActionResult ConfirmDelete(int id)
		{
			var welcome = db.GetWelcome(id);
			if (welcome != null)
				return View(welcome);
			return NotFound();
		}

		// POST: WelcomeController/Delete/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete(int id)
		{
			try
			{
				db.DeleteWelcome(id);
				return RedirectToAction(nameof(Index));
			}
			catch
			{
				return View();
			}
		}
	}
}
