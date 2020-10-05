using Fuselage;
using Fuselage.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Linq;

namespace Ex077.Controllers
{
	[Authorize]
	public class WelcomeController : Controller
	{
		private readonly FuselageContext _fuselage;
		public WelcomeController(FuselageContext fuselage)
		{
			_fuselage = fuselage;
		}

		public ActionResult Index()
		{
			return View(_fuselage.Welcomes);
		}

		#region Create
		public IActionResult Create() => View();

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Welcome welcome)
		{
			_fuselage.Add(welcome);
			_fuselage.SaveChanges();
			return RedirectToAction(nameof(Index));
		}
		#endregion

		#region Edit
		public IActionResult Edit(int id)
		{
			var welcome = _fuselage.Welcomes.FirstOrDefault(s => s.Id == id);

			if (welcome != null)
				return View(welcome);

			return NotFound();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(Welcome welcome)
		{
			_fuselage.Welcomes.Update(welcome);
			_fuselage.SaveChanges();
			return RedirectToAction(nameof(Index));
		}
		#endregion

		#region Delete
		public IActionResult Delete(int id)
		{
			var welcome = _fuselage.Welcomes.FirstOrDefault(s => s.Id == id);

			if (welcome == null)
				return NotFound();

			_fuselage.Welcomes.Remove(welcome);
			_fuselage.SaveChanges();
			return RedirectToAction(nameof(Index));
		}
		#endregion


	}
}
