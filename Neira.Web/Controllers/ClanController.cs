using ChartJSCore.Helpers;
using ChartJSCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Neira.Web.Models.NeiraLink;
using Neira.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Web.Controllers
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

			using var Db = new NeiraContext();
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

		public IActionResult Detail(int id)
		{
			//Get data for Chart.js
			Chart lineChart = GenerateLineChart(id);
			ViewData["LineChart"] = lineChart;

			using var Db = new NeiraContext();
			var model = new GuardianViewModel
			{
				GuardianInfo = Db.Clan_Members.FirstOrDefault(m => m.Id == id)
			};
			return View(model);
		}

		private Chart GenerateLineChart(int GuardianId)
		{
			using var Db = new NeiraContext();

			var result = Db.Clan_Member_Stats.Where(m => m.MemberId == GuardianId).OrderBy(o => o.Date);

			var chart = new Chart
			{
				Type = Enums.ChartType.Line
			};

			var data = new Data
			{
				Labels = new List<string>(result.Select(r => r.Date.ToString("dd.MM.yyyy")))
			};

			var GuardianDataset = new LineDataset()
			{
				Label = "Активность(Часов)",
				Data = new List<double>(result.Select(r => Math.Round(TimeSpan.FromSeconds(r.PlayedTime).TotalHours, 1, MidpointRounding.AwayFromZero))),
				Fill = "false",
				BackgroundColor = ChartColor.FromRgba(75, 192, 192, 0.4),
				BorderColor = ChartColor.FromRgb(75, 192, 192),
				LineTension = 0.1,
				BorderCapStyle = "butt",
				BorderDash = new List<int> { },
				BorderDashOffset = 0.0,
				BorderJoinStyle = "miter",
				PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
				PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
				PointBorderWidth = new List<int> { 1 },
				PointHoverRadius = new List<int> { 5 },
				PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
				PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
				PointHoverBorderWidth = new List<int> { 2 },
				PointRadius = new List<int> { 1 },
				PointHitRadius = new List<int> { 10 },
				SpanGaps = false
			};

			//Add in chart two datasets
			data.Datasets = new List<Dataset>
			{
				GuardianDataset
			};

			chart.Data = data;

			return chart;
		}
		private bool ClanExists(long id)
		{
			using var Db = new NeiraContext();
			return Db.Clans.Any(e => e.Id == id);
		}
	}
}
