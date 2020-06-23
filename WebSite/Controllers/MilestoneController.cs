using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Neiralink;
using Neiralink.Models;

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

		// GET: MilestoneController
		public ActionResult Index()
		{
			return View(db.GetAllMilestoneInfos());
		}

		// GET: MilestoneController/Details/5
		public ActionResult Details(int id)
		{
			return View();
		}

		// GET: MilestoneController/Create
		public ActionResult Create()
		{
			return View();
		}

		// POST: MilestoneController/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
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

		// GET: MilestoneController/Edit/5
		public ActionResult Edit(byte id)
		{
			var milestone = db.GetMilestoneInfo(id);
			if (milestone != null)
				return View(milestone);
			return NotFound();
		}

		// POST: MilestoneController/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
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

		// GET: MilestoneController/Delete/5
		[ActionName("Delete")]
		public ActionResult ConfirmDelete(byte id)
		{
			var milestone = db.GetMilestoneInfo(id);
			if (milestone != null)
				return View(milestone);
			return NotFound();
		}

		// POST: MilestoneController/Delete/5
		[HttpPost]
		[ValidateAntiForgeryToken]
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
	}
}
