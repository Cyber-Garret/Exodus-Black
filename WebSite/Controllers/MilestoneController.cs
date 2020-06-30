using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Neiralink;
using Neiralink.Models;

using WebSite.ViewModels;

namespace WebSite.Controllers
{
	[Authorize]
	public class MilestoneController : Controller
	{
		private readonly IMilestoneDbClient db;
		public MilestoneController(IMilestoneDbClient milestoneDbClient)
		{
			db = milestoneDbClient;
		}

		public ActionResult Index()
		{
			return View(db.GetAllMilestoneInfos());
		}

		#region Main
		public ActionResult Create()
		{
			return View();
		}
		[HttpPost, ValidateAntiForgeryToken]
		public ActionResult Create(MilestoneInfo milestone)
		{
			try
			{
				db.AddMilestone(milestone);
				return RedirectToAction(nameof(Index));
			}
			catch
			{
				return View();
			}
		}

		public ActionResult Edit(byte id)
		{
			var milestone = db.GetMilestoneInfo(id);
			if (milestone != null)
				return View(new MilestoneViewModel { Info = milestone, Locales = db.GetMilestoneLocales(id).ToList() });
			return NotFound();
		}
		[HttpPost, ValidateAntiForgeryToken]
		public ActionResult Edit(MilestoneInfo milestone)
		{
			try
			{
				db.UpdateMilestone(milestone);
				return RedirectToAction(nameof(Index));
			}
			catch
			{
				return View();
			}
		}

		[ActionName("Delete")]
		public ActionResult ConfirmDelete(byte id)
		{
			var milestone = db.GetMilestoneInfo(id);
			if (milestone != null)
				return View(new MilestoneViewModel { Info = milestone, Locales = db.GetMilestoneLocales(id).ToList() });
			return NotFound();
		}
		[HttpPost, ValidateAntiForgeryToken]
		public ActionResult Delete(byte id)
		{
			try
			{
				db.DeleteMilestone(id);
				return RedirectToAction(nameof(Index));
			}
			catch
			{
				return View();
			}
		}
		#endregion

		#region Locale
		public ActionResult CreateLocale(byte id)
		{
			if (id > 0)
			{
				var model = new MilestoneInfoLocale { MilestoneInfoRowID = id };
				return View(model);
			}
			return NotFound();
		}
		[HttpPost, ValidateAntiForgeryToken]
		public ActionResult CreateLocale(MilestoneInfoLocale locale)
		{
			if (ModelState.IsValid)
			{
				try
				{
					db.AddMilestoneLocale(locale);
					return RedirectToAction(nameof(Edit), new { id = locale.MilestoneInfoRowID });
				}
				catch
				{
					return View();
				}
			}
			else
				return View();
		}

		public ActionResult EditLocale(byte id, LangKey lang)
		{
			var locale = db.GetMilestoneLocale(id, lang);
			if (locale != null)
				return View(locale);
			return NotFound();
		}
		[HttpPost, ValidateAntiForgeryToken]
		public ActionResult EditLocale(MilestoneInfoLocale locale)
		{
			if (ModelState.IsValid)
			{
				try
				{
					db.UpdateMilestoneLocale(locale);
					return RedirectToAction(nameof(Edit), new { id = locale.MilestoneInfoRowID });
				}
				catch
				{
					return View();
				}
			}
			return RedirectToAction(nameof(Index));
		}

		public ActionResult DeleteLocale(byte id, LangKey lang)
		{
			try
			{
				db.DeleteMilestoneLocale(id, lang);
				return RedirectToAction(nameof(Edit), new { id });
			}
			catch
			{
				return RedirectToAction(nameof(Index));
			}
		}
		#endregion

	}
}
