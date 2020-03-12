using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Web.Models;

namespace Web.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> logger;
		private readonly IBotRepository bot;
		public BotStat botStat;

		public IndexModel(ILogger<IndexModel> logger, IBotRepository botRepository)
		{
			bot = botRepository;
			this.logger = logger;
		}

		public void OnGet()
		{
			var stat = bot.GetBotStat(1);
			if (stat == null)
				logger.LogWarning("Bot stat empty");
			botStat = stat;
		}

		public IActionResult OnGetAddBot() => RedirectPermanent($"https://discordapp.com/oauth2/authorize?client_id=521693707238506498&scope=bot&permissions=269479104");
		public IActionResult OnGetDocs() => RedirectPermanent($"https://docs.neira.su/");
		public IActionResult OnGetBlackExodus() => RedirectPermanent($"https://discord.gg/WcuNPM9");
		public IActionResult OnGetYandexMoney() => RedirectPermanent($"https://money.yandex.ru/to/410019748161790");

		public IActionResult OnGetGithub() => RedirectPermanent("https://github.com/Cyber-Garret");
	}
}
