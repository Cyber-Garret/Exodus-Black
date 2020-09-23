using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Neiralink;
using Neiralink.Enums;
using Neiralink.Models;

using System.Linq;

namespace WebSite.Controllers
{
	[Authorize]
	public class CatalystController : Controller
	{
		private readonly ICatalystDbClient db;
		public CatalystController(ICatalystDbClient catalystDb)
		{
			db = catalystDb;
		}

		public IActionResult Index()
		{
			return View(db.GetCatalysts().ToList());
		}

		public IActionResult Create()
		{
			return View();
		}
		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult Create(Catalyst catalyst)
		{
			try
			{
				db.AddCatalyst(catalyst);
				return RedirectToAction(nameof(Index));
			}
			catch
			{
				return View();
			}
		}

		public IActionResult Edit(int id, LangKey lang)
		{
			var catalyst = db.GetCatalyst(id, lang);
			if (catalyst != null)
				return View(catalyst);
			return NotFound();
		}
		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult Edit(Catalyst catalyst)
		{
			try
			{
				db.UpdateCatalyst(catalyst);
				return RedirectToAction(nameof(Index));
			}
			catch
			{
				return View();
			}
		}

		[ActionName("Delete")]
		public IActionResult ConfirmDelete(int id, LangKey lang)
		{
			var catalyst = db.GetCatalyst(id, lang);
			if (catalyst != null)
				return View(catalyst);
			return NotFound();
		}
		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult Delete(int id, LangKey lang)
		{
			try
			{
				db.DeleteCatalyst(id, lang);
				return RedirectToAction(nameof(Index));
			}
			catch
			{
				return View();
			}
		}
	}
}
