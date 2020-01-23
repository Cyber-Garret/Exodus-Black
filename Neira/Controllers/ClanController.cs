using ChartJSCore.Helpers;
using ChartJSCore.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Neira.Database;
using Neira.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Controllers
{
	public class ClanController : Controller
	{

		// GET: /Clan/2733538
		[Route("Clan/{id}/")]
		public async Task<IActionResult> Index(long? id, string sortOrder, string searchString)
		{
			if (id == null)
				return NotFound();

			if (!ClanExists(id.Value))
				return NotFound();

			using var Db = new NeiraLinkContext();
			ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "Name_desc" : "";
			ViewData["DateSortParm"] = sortOrder == "Date" ? "Date_desc" : "Date";
			ViewData["DateLastPlayedSortParm"] = sortOrder == "DateLastPlayed" ? "DateLastPlayed_desc" : "DateLastPlayed";
			ViewData["CurrentFilter"] = searchString;
			ViewData["CurrentGuild"] = Db.Clans.AsNoTracking().FirstOrDefault(C => C.Id == id);

			var destiny2Clan = from d in Db.Clan_Members.Include(c => c.Clan)
							   where d.ClanId == id
							   select d;

			if (!string.IsNullOrEmpty(searchString))
				destiny2Clan = destiny2Clan.Where(m => m.Name.Contains(searchString)).Include(c => c.Clan);

			if (destiny2Clan == null)
				return NotFound();

			destiny2Clan = sortOrder switch
			{
				"Name_desc" => destiny2Clan.OrderByDescending(m => m.Name).Include(c => c.Clan),
				"Date" => destiny2Clan.OrderBy(m => m.ClanJoinDate).Include(c => c.Clan),
				"Date_desc" => destiny2Clan.OrderByDescending(m => m.ClanJoinDate).Include(c => c.Clan),
				"DateLastPlayed" => destiny2Clan.OrderBy(m => m.DateLastPlayed).Include(c => c.Clan),
				"DateLastPlayed_desc" => destiny2Clan.OrderByDescending(m => m.DateLastPlayed).Include(c => c.Clan),
				_ => destiny2Clan.OrderBy(m => m.Name).Include(c => c.Clan),
			};

			return View(await destiny2Clan.AsNoTracking().ToListAsync());
		}

		[Route("OpenProfile")]
		public IActionResult OpenProfile(long type, string id) => RedirectPermanent($"https://www.bungie.net/ru/Profile/{type}/{id}/");

		[Route("OpenClan")]
		public IActionResult OpenClan(long id) => RedirectPermanent($"https://www.bungie.net/ru/ClanV2?groupid={id}");

		public async Task<IActionResult> ADAsync(string sortOrder, string searchString)
		{
			using var Db = new NeiraLinkContext();
			ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "Name_desc" : "";
			ViewData["DateSortParm"] = sortOrder == "Date" ? "Date_desc" : "Date";
			ViewData["DateLastPlayedSortParm"] = sortOrder == "DateLastPlayed" ? "DateLastPlayed_desc" : "DateLastPlayed";
			ViewData["CurrentFilter"] = searchString;

			var AD = await Db.Clans.Where(c => c.Name.Contains("Адские")).Select(i => i.Id).ToListAsync();
			var destiny2Clan = Db.Clan_Members.Include(c => c.Clan).Where(i => AD.Contains(i.ClanId));

			if (!string.IsNullOrEmpty(searchString))
				destiny2Clan = destiny2Clan.Where(m => m.Name.Contains(searchString)).Include(c => c.Clan);

			if (destiny2Clan == null)
				return NotFound();

			destiny2Clan = sortOrder switch
			{
				"Name_desc" => destiny2Clan.OrderByDescending(m => m.Name).Include(c => c.Clan),
				"Date" => destiny2Clan.OrderBy(m => m.ClanJoinDate).Include(c => c.Clan),
				"Date_desc" => destiny2Clan.OrderByDescending(m => m.ClanJoinDate).Include(c => c.Clan),
				"DateLastPlayed" => destiny2Clan.OrderBy(m => m.DateLastPlayed).Include(c => c.Clan),
				"DateLastPlayed_desc" => destiny2Clan.OrderByDescending(m => m.DateLastPlayed).Include(c => c.Clan),
				_ => destiny2Clan.OrderBy(m => m.Name).Include(c => c.Clan),
			};

			return View(await destiny2Clan.AsNoTracking().ToListAsync());
		}

		private bool ClanExists(long id)
		{
			using var Db = new NeiraLinkContext();
			return Db.Clans.Any(e => e.Id == id);
		}
	}
}
