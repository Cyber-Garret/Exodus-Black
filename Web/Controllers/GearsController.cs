using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Web.Helpers;

using Core;
using Core.Models.Db;
using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers
{
	[Authorize(Roles = "admin")]
	public class GearsController : Controller
	{
		private readonly FailsafeContext _context;

		public GearsController(FailsafeContext context)
		{
			_context = context;
		}

		// GET: Gears
		[Authorize]
		public async Task<IActionResult> Index(
			string sortOrder,
			string currentFilter,
			string searchString,
			int? pageNumber)
		{
			ViewData["CurrentSort"] = sortOrder;
			ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
			ViewData["CatalystSortParm"] = sortOrder == "Catalyst" ? "catalyst_desc" : "Catalyst";

			if (searchString != null)
			{
				pageNumber = 1;
			}
			else
			{
				searchString = currentFilter;
			}

			ViewData["CurrentFilter"] = searchString;

			var gears = from g in _context.Gears
						select g;

			if (!string.IsNullOrEmpty(searchString))
			{
				gears = gears.Where(s => s.Name.ToLower().Contains(searchString.ToLower())
									   || s.Type.ToLower().Contains(searchString.ToLower()));
			}

			switch (sortOrder)
			{
				case "name_desc":
					gears = gears.OrderByDescending(s => s.Name);
					break;
				case "Catalyst":
					gears = gears.OrderBy(s => s.Catalyst);
					break;
				case "catalyst_desc":
					gears = gears.OrderByDescending(s => s.Catalyst);
					break;
				default:
					gears = gears.OrderBy(s => s.Name);
					break;
			}

			int pageSize = 50;
			return View(await PaginatedList<Gear>.CreateAsync(gears.AsNoTracking(), pageNumber ?? 1, pageSize));
		}

		// GET: Gears/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var gear = await _context.Gears
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id);
			if (gear == null)
			{
				return NotFound();
			}

			return View(gear);
		}

		// GET: Gears/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Gears/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Name,Type,IconUrl,ImageUrl,Description,PerkName,PerkDescription,SecondPerkName,SecondPerkDescription,DropLocation,Catalyst,WhereCatalystDrop,CatalystQuest,CatalystBonus")] Gear gear)
		{
			try
			{
				if (ModelState.IsValid)
				{
					_context.Add(gear);
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException /* ex */)
			{
				//Логгирование ошибки (Раскомментируй переменную ex и реализуй логгирование).
				ModelState.AddModelError("", "Неудалось сохранить изменения. " +
					"Попробуй немного позже, если ошибка все еще будет " +
					"сообщи администратору.");
			}
			return View(gear);
		}

		// GET: Gears/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var gear = await _context.Gears.FindAsync(id);
			if (gear == null)
			{
				return NotFound();
			}
			return View(gear);
		}

		// POST: Gears/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost, ActionName("Edit")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditGear(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}
			var GearToUpdate = await _context.Gears.FirstOrDefaultAsync(s => s.Id == id);
			if (await TryUpdateModelAsync(
				GearToUpdate,
				"",
				g => g.Name,
				g => g.Type,
				g => g.IconUrl,
				g => g.ImageUrl,
				g => g.Description,
				g => g.PerkName,
				g => g.PerkDescription,
				g => g.SecondPerkName,
				g => g.SecondPerkDescription,
				g => g.DropLocation,
				g => g.Catalyst,
				g => g.WhereCatalystDrop,
				g => g.CatalystQuest,
				g => g.CatalystBonus
				))
			{
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateException /* ex */)
				{
					//Log the error (uncomment ex variable name and write a log.)
					ModelState.AddModelError("", "Неудалось сохранить изменения. " +
						"Попробуй немного позже, если ошибка все еще будет " +
						"сообщи администратору.");
				}
			}
			return View(GearToUpdate);
		}

		// GET: Gears/Delete/5
		public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
		{
			if (id == null)
			{
				return NotFound();
			}

			var gear = await _context.Gears
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.Id == id);
			if (gear == null)
			{
				return NotFound();
			}

			if (saveChangesError.GetValueOrDefault())
			{
				ViewData["ErrorMessage"] =
					"Ошибка удаления. Попробуй немного позже, если ошибка все еще будет " +
					"сообщи администратору.";
			}

			return View(gear);
		}

		// POST: Gears/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var gear = await _context.Gears.FindAsync(id);
			if (gear == null)
			{
				return RedirectToAction(nameof(Index));
			}
			try
			{
				_context.Gears.Remove(gear);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			catch (DbUpdateException /* ex */)
			{
				//Логгирование ошибки (Раскомментируй переменную ex и реализуй логгирование).
				return RedirectToAction(nameof(Delete), new { id, saveChangesError = true });
			}

		}

		private bool GearExists(int id)
		{
			return _context.Gears.Any(e => e.Id == id);
		}
	}
}
