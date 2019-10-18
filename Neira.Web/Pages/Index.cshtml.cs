using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Neira.Web.Models;

namespace Neira.Web.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;
		private readonly NeiraContext Db;
		public BotInfo BotInfo { get; set; }

		public IndexModel(ILogger<IndexModel> logger, NeiraContext neiraContext)
		{
			_logger = logger;
			Db = neiraContext;
		}

		public void OnGet()
		{
			BotInfo = Db.BotInfos.FirstOrDefault();
		}
	}
}
