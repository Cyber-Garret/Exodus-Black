using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Web.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;

		public IndexModel(ILogger<IndexModel> logger)
		{
			_logger = logger;
		}

		public void OnGet()
		{

		}

		public IActionResult OnGetAddBot() => RedirectPermanent($"https://discordapp.com/oauth2/authorize?client_id=521693707238506498&scope=bot&permissions=269479104");
		public IActionResult OnGetDocs() => RedirectPermanent($"https://docs.neira.su/");
		public IActionResult OnGetBlackExodus() => RedirectPermanent($"https://discord.gg/WcuNPM9");
		public IActionResult OnGetYandexMoney() => RedirectPermanent($"https://money.yandex.ru/to/410019748161790");
	}
}
