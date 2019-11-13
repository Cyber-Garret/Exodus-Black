using ChartJSCore.Helpers;
using ChartJSCore.Models;

using Microsoft.AspNetCore.Mvc;

using Neira.Web.Models.NeiraLink;

using System.Collections.Generic;
using System.Linq;

namespace Neira.Web.Controllers
{
	public class ADController : Controller
	{
		public IActionResult Index()
		{
			//Get data for Chart.js
			Chart lineChart = GenerateLineChart();
			ViewData["LineChart"] = lineChart;

			return View();
		}

		private Chart GenerateLineChart()
		{
			using var Db = new NeiraContext();

			var result = Db.ADOnlines.OrderBy(o => o.Date);

			var chart = new Chart
			{
				Type = Enums.ChartType.Line
			};

			var data = new Data
			{
				Labels = new List<string>(result.Select(r => r.Date.ToString("HH:mm dd.MM.yyyy")))
			};
			//Online
			var Online = new LineDataset()
			{
				Label = "Онлайн",
				Data = new List<double>(result.Select(r => (double)r.Online)),
				Fill = "false",
				BackgroundColor = ChartColor.FromRgba(67, 181, 129, 0.4),
				BorderColor = ChartColor.FromRgb(67, 181, 129),
				LineTension = 0.1,
				BorderCapStyle = "butt",
				BorderDash = new List<int> { },
				BorderDashOffset = 0.0,
				BorderJoinStyle = "miter",
				PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(67, 181, 129) },
				PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
				PointBorderWidth = new List<int> { 1 },
				PointHoverRadius = new List<int> { 5 },
				PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(67, 181, 129) },
				PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
				PointHoverBorderWidth = new List<int> { 2 },
				PointRadius = new List<int> { 1 },
				PointHitRadius = new List<int> { 10 },
				SpanGaps = false
			};
			//Idle
			var Idle = new LineDataset()
			{
				Label = "Не активен",
				Data = new List<double>(result.Select(r => (double)r.Idle)),
				Fill = "false",
				BackgroundColor = ChartColor.FromRgba(250, 166, 26, 0.4),
				BorderColor = ChartColor.FromRgb(250, 166, 26),
				LineTension = 0.1,
				BorderCapStyle = "butt",
				BorderDash = new List<int> { },
				BorderDashOffset = 0.0,
				BorderJoinStyle = "miter",
				PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(250, 166, 26) },
				PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
				PointBorderWidth = new List<int> { 1 },
				PointHoverRadius = new List<int> { 5 },
				PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(250, 166, 26) },
				PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
				PointHoverBorderWidth = new List<int> { 2 },
				PointRadius = new List<int> { 1 },
				PointHitRadius = new List<int> { 10 },
				SpanGaps = false
			};
			//DnD
			var DnD = new LineDataset()
			{
				Label = "Не беспокоить",
				Data = new List<double>(result.Select(r => (double)r.DnD)),
				Fill = "false",
				BackgroundColor = ChartColor.FromRgba(240, 71, 71, 0.4),
				BorderColor = ChartColor.FromRgb(240, 71, 71),
				LineTension = 0.1,
				BorderCapStyle = "butt",
				BorderDash = new List<int> { },
				BorderDashOffset = 0.0,
				BorderJoinStyle = "miter",
				PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(240, 71, 71) },
				PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
				PointBorderWidth = new List<int> { 1 },
				PointHoverRadius = new List<int> { 5 },
				PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(240, 71, 71) },
				PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
				PointHoverBorderWidth = new List<int> { 2 },
				PointRadius = new List<int> { 1 },
				PointHitRadius = new List<int> { 10 },
				SpanGaps = false
			};
			//InVoice
			var InVoice = new LineDataset()
			{
				Label = "В голосе",
				Data = new List<double>(result.Select(r => (double)r.InVoice)),
				Fill = "false",
				BackgroundColor = ChartColor.FromRgba(0, 230, 0, 0.4),
				BorderColor = ChartColor.FromRgb(0, 230, 0),
				LineTension = 0.1,
				BorderCapStyle = "butt",
				BorderDash = new List<int> { },
				BorderDashOffset = 0.0,
				BorderJoinStyle = "miter",
				PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(0, 230, 0) },
				PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
				PointBorderWidth = new List<int> { 1 },
				PointHoverRadius = new List<int> { 5 },
				PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(0, 230, 0) },
				PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
				PointHoverBorderWidth = new List<int> { 2 },
				PointRadius = new List<int> { 1 },
				PointHitRadius = new List<int> { 10 },
				SpanGaps = false
			};
			//AFK
			var AFK = new LineDataset()
			{
				Label = "Отошел",
				Data = new List<double>(result.Select(r => (double)r.AFK)),
				Fill = "false",
				BackgroundColor = ChartColor.FromRgba(253, 225, 178, 0.4),
				BorderColor = ChartColor.FromRgb(253, 225, 178),
				LineTension = 0.1,
				BorderCapStyle = "butt",
				BorderDash = new List<int> { },
				BorderDashOffset = 0.0,
				BorderJoinStyle = "miter",
				PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(253, 225, 178) },
				PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
				PointBorderWidth = new List<int> { 1 },
				PointHoverRadius = new List<int> { 5 },
				PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(253, 225, 178) },
				PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
				PointHoverBorderWidth = new List<int> { 2 },
				PointRadius = new List<int> { 1 },
				PointHitRadius = new List<int> { 10 },
				SpanGaps = false
			};
			//InGame
			var InGame = new LineDataset()
			{
				Label = "В игре(любая)",
				Data = new List<double>(result.Select(r => (double)r.InGame)),
				Fill = "false",
				BackgroundColor = ChartColor.FromRgba(255, 26, 255, 0.4),
				BorderColor = ChartColor.FromRgb(255, 26, 255),
				LineTension = 0.1,
				BorderCapStyle = "butt",
				BorderDash = new List<int> { },
				BorderDashOffset = 0.0,
				BorderJoinStyle = "miter",
				PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(255, 26, 255) },
				PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
				PointBorderWidth = new List<int> { 1 },
				PointHoverRadius = new List<int> { 5 },
				PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(255, 26, 255) },
				PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(255, 26, 255) },
				PointHoverBorderWidth = new List<int> { 2 },
				PointRadius = new List<int> { 1 },
				PointHitRadius = new List<int> { 10 },
				SpanGaps = false
			};
			//InDestiny2
			var InDestiny2 = new LineDataset()
			{
				Label = "В игре(Destiny 2)",
				Data = new List<double>(result.Select(r => (double)r.InDestiny2)),
				Fill = "false",
				BackgroundColor = ChartColor.FromRgba(128, 0, 128, 0.4),
				BorderColor = ChartColor.FromRgb(128, 0, 128),
				LineTension = 0.1,
				BorderCapStyle = "butt",
				BorderDash = new List<int> { },
				BorderDashOffset = 0.0,
				BorderJoinStyle = "miter",
				PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(128, 0, 128) },
				PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
				PointBorderWidth = new List<int> { 1 },
				PointHoverRadius = new List<int> { 5 },
				PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(128, 0, 128) },
				PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
				PointHoverBorderWidth = new List<int> { 2 },
				PointRadius = new List<int> { 1 },
				PointHitRadius = new List<int> { 10 },
				SpanGaps = false
			};
			//Add in chart two datasets
			data.Datasets = new List<Dataset>
			{
				Online,
				Idle,
				DnD,
				InVoice,
				AFK,
				InGame,
				InDestiny2
			};

			chart.Data = data;

			return chart;
		}
	}
}