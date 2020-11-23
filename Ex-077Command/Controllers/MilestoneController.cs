using Fuselage;
using Fuselage.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace Ex077.Controllers
{
	[Authorize]
	public class MilestoneController : Controller
	{
		private readonly FuselageContext _fuselage;
		public MilestoneController(FuselageContext fuselage)
		{
			_fuselage = fuselage;
		}

		public IActionResult Index() => View(_fuselage.Milestones.Include(j => j.MilestoneLocales).AsNoTracking());

		#region Main
		public IActionResult Create() => View();

		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult Create(Milestone milestone)
		{
			_fuselage.Milestones.Add(milestone);
			_fuselage.SaveChanges();
			return RedirectToAction(nameof(Index));
		}

		public PartialViewResult Detail(int id)
		{
			var model = _fuselage.Milestones.Include(n => n.MilestoneLocales).FirstOrDefault(s => s.Id == id);
			if (model != null)
				return PartialView("_Detail", model);
			return null;
		}
		public IActionResult Edit(int id)
		{
			var milestone = _fuselage.Milestones.FirstOrDefault(s => s.Id == id);
			if (milestone != null)
				return View(milestone);
			return NotFound();
		}

		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult Edit(Milestone milestone)
		{
			_fuselage.Milestones.Update(milestone);
			_fuselage.SaveChanges();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Delete(int id)
		{
			var milsetone = _fuselage.Milestones.FirstOrDefault(s => s.Id == id);
			if (milsetone != null)
			{
				_fuselage.Milestones.Remove(milsetone);
				_fuselage.SaveChanges();
				return RedirectToAction(nameof(Index));
			}
			return NotFound();
		}
		#endregion

		#region Locale
		public IActionResult CreateLocale(int id)
		{
			var milestone = _fuselage.Milestones.AsNoTracking().FirstOrDefault(s => s.Id == id);
			if (milestone != null)
			{
				var model = new MilestoneLocale { MilestoneId = milestone.Id, Milestone = milestone };
				return View(model);
			}
			return NotFound();
		}

		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult CreateLocale(MilestoneLocale milestoneLocale)
		{
			if (ModelState.IsValid)
			{
				//EF Core crutch
				milestoneLocale.Id = 0;

				_fuselage.MilestoneLocales.Add(milestoneLocale);
				_fuselage.SaveChanges();
				return RedirectToAction(nameof(Index));
			}
			else return View(milestoneLocale);
		}

		public IActionResult EditLocale(int id)
		{
			var locale = _fuselage.MilestoneLocales.FirstOrDefault(s => s.Id == id);
			if (locale != null)
				return View(locale);
			return NotFound();
		}
		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult EditLocale(MilestoneLocale milestoneLocale)
		{
			_fuselage.MilestoneLocales.Update(milestoneLocale);
			_fuselage.SaveChanges();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult DeleteLocale(int id)
		{
			var milsetone = _fuselage.MilestoneLocales.FirstOrDefault(s => s.Id == id);
			if (milsetone != null)
			{
				_fuselage.MilestoneLocales.Remove(milsetone);
				_fuselage.SaveChanges();
				return RedirectToAction(nameof(Index));
			}
			return NotFound();
		}
		#endregion

	}
}
