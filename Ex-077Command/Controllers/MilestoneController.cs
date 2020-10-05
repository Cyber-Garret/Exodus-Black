using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Fuselage;
using Fuselage.Models;

using System;
using System.Linq;

using Ex077.ViewModels;
using Microsoft.EntityFrameworkCore;

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

		public IActionResult Index() => View(_fuselage.Milestones);

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
		//public IActionResult CreateLocale(byte id)
		//{
		//	if (id > 0)
		//	{
		//		var model = new MilestoneInfoLocale { MilestoneInfoRowID = id };
		//		return View(model);
		//	}
		//	return NotFound();
		//}
		//[HttpPost, ValidateAntiForgeryToken]
		//public IActionResult CreateLocale(MilestoneInfoLocale locale)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		try
		//		{
		//			_fuselage.AddMilestoneLocale(locale);
		//			return RedirectToAction(nameof(Edit), new { id = locale.MilestoneInfoRowID });
		//		}
		//		catch
		//		{
		//			return View();
		//		}
		//	}
		//	else
		//		return View();
		//}

		//public IActionResult EditLocale(byte id, LangKey lang)
		//{
		//	var locale = _fuselage.GetMilestoneLocale(id, lang);
		//	if (locale != null)
		//		return View(locale);
		//	return NotFound();
		//}
		//[HttpPost, ValidateAntiForgeryToken]
		//public IActionResult EditLocale(MilestoneInfoLocale locale)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		try
		//		{
		//			_fuselage.UpdateMilestoneLocale(locale);
		//			return RedirectToAction(nameof(Edit), new { id = locale.MilestoneInfoRowID });
		//		}
		//		catch
		//		{
		//			return View();
		//		}
		//	}
		//	return RedirectToAction(nameof(Index));
		//}

		//public IActionResult DeleteLocale(byte id, LangKey lang)
		//{
		//	try
		//	{
		//		_fuselage.DeleteMilestoneLocale(id, lang);
		//		return RedirectToAction(nameof(Edit), new { id });
		//	}
		//	catch
		//	{
		//		return RedirectToAction(nameof(Index));
		//	}
		//}
		#endregion

	}
}
