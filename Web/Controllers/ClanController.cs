using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Core.Models.Db;
using API.Bungie;

namespace Web.Controllers
{
	public class ClanController : Controller
	{
		private readonly FailsafeContext _context;

		public ClanController(FailsafeContext context)
		{
			_context = context;
		}

		// GET: Clan
		public async Task<IActionResult> Index(string sortOrder)
		{
			ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
			ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
			ViewData["CountSortParm"] = sortOrder == "Count" ? "count_desc" : "Count";
			var clans = from c in _context.Destiny2Clans.Include(m => m.Members)
						select c;

			switch (sortOrder)
			{
				case "name_desc":
					clans = clans.OrderByDescending(c => c.Name);
					break;
				case "Date":
					clans = clans.OrderBy(c => c.CreateDate);
					break;
				case "date_desc":
					clans = clans.OrderByDescending(c => c.CreateDate);
					break;
				case "Count":
					clans = clans.OrderBy(c => c.MemberCount);
					break;
				case "count_desc":
					clans = clans.OrderByDescending(c => c.MemberCount);
					break;
				default:
					clans = clans.OrderBy(s => s.Name);
					break;
			}

			return View(await clans.AsNoTracking().ToListAsync());
		}

		// GET: Clan/Details/5
		public async Task<IActionResult> Details(long? id, string sortOrder, string searchString)
		{
			ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "Name_desc" : "";
			ViewData["DateSortParm"] = sortOrder == "Date" ? "Date_desc" : "Date";
			ViewData["DateLastPlayedSortParm"] = sortOrder == "DateLastPlayed" ? "DateLastPlayed_desc" : "DateLastPlayed";
			ViewData["CurrentFilter"] = searchString;
			ViewData["CurrentGuild"] = _context.Destiny2Clans.AsNoTracking().FirstOrDefault(C => C.Id == id);
			if (id == null)
			{
				return NotFound();
			}

			var destiny2Clan = from d in _context.Destiny2Clan_Members.Include(c => c.Destiny2Clan)
							   where d.Destiny2ClanId == id
							   select d;
			if (!string.IsNullOrEmpty(searchString))
			{
				destiny2Clan = destiny2Clan.Where(m => m.Name.Contains(searchString)).Include(c => c.Destiny2Clan);
			}

			if (destiny2Clan == null)
			{
				return NotFound();
			}

			switch (sortOrder)
			{
				case "Name_desc":
					destiny2Clan = destiny2Clan.OrderByDescending(m => m.Name).Include(c => c.Destiny2Clan);
					break;
				case "Date":
					destiny2Clan = destiny2Clan.OrderBy(m => m.ClanJoinDate).Include(c => c.Destiny2Clan);
					break;
				case "Date_desc":
					destiny2Clan = destiny2Clan.OrderByDescending(m => m.ClanJoinDate).Include(c => c.Destiny2Clan);
					break;
				case "DateLastPlayed":
					destiny2Clan = destiny2Clan.OrderBy(m => m.DateLastPlayed).Include(c => c.Destiny2Clan);
					break;
				case "DateLastPlayed_desc":
					destiny2Clan = destiny2Clan.OrderByDescending(m => m.DateLastPlayed).Include(c => c.Destiny2Clan);
					break;
				default:
					destiny2Clan = destiny2Clan.OrderBy(m => m.Name).Include(c => c.Destiny2Clan);
					break;
			}

			return View(await destiny2Clan.AsNoTracking().ToListAsync());
		}

		// GET: Clan/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Clan/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id")] Destiny2Clan destiny2Clan)
		{
			if (Destiny2ClanExists(destiny2Clan.Id))
				return View(destiny2Clan);

			if (ModelState.IsValid)
			{
				BungieApi bungie = new BungieApi();
				var claninfo = bungie.GetGroupResult(destiny2Clan.Id);
				if (Response != null)
				{
					destiny2Clan.Id = claninfo.Response.Detail.GroupId;
					destiny2Clan.Name = claninfo.Response.Detail.Name;
					destiny2Clan.CreateDate = claninfo.Response.Detail.CreationDate;
					destiny2Clan.Motto = claninfo.Response.Detail.Motto;
					destiny2Clan.About = claninfo.Response.Detail.About;
					destiny2Clan.MemberCount = claninfo.Response.Detail.MemberCount;

					_context.Add(destiny2Clan);
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
				else
				{
					return View(destiny2Clan);
				}
			}
			return View(destiny2Clan);
		}

		private bool Destiny2ClanExists(long id)
		{
			return _context.Destiny2Clans.Any(e => e.Id == id);
		}
	}
}
