using Fuselage;
using Fuselage.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Linq;

namespace WebSite.Controllers
{
	[Authorize]
	public class CatalystController : Controller
	{
		private readonly FuselageContext _fuselage;
		public CatalystController(FuselageContext fuselage)
		{
			_fuselage = fuselage;
		}

		public IActionResult Index()
		{
			return View(_fuselage.Catalysts);
		}

		public IActionResult Create()
		{
			return View();
		}
		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult Create(Catalyst catalyst)
		{
			_fuselage.Catalysts.Add(catalyst);
			_fuselage.SaveChanges();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Edit(int id)
		{
			var catalyst = _fuselage.Catalysts.FirstOrDefault(s => s.Id == id);
			if (catalyst != null)
				return View(catalyst);
			return NotFound();
		}
		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult Edit(Catalyst catalyst)
		{
			_fuselage.Catalysts.Update(catalyst);
			_fuselage.SaveChanges();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Delete(int id)
		{
			var catalyst = _fuselage.Catalysts.FirstOrDefault(s => s.Id == id);
			if (catalyst != null)
			{
				_fuselage.Catalysts.Remove(catalyst);
				_fuselage.SaveChanges();
				return RedirectToAction(nameof(Index));
			}
			return NotFound();
		}
	}
}
