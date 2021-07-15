using Ex077.Entities;
using Ex077.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using System;
using System.Diagnostics;

namespace Ex077.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptions<BotOptions> _options;

        public HomeController(IOptions<BotOptions> options)
        {
            _options = options;
        }

        #region GET
        public IActionResult Index()
        {
            return View(new HomeViewModel());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion

        #region POST
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
        #endregion

        #region Routes

        [Route("BlackExodus")]
        public IActionResult BlackExodus() =>
            RedirectPermanent(_options.Value.ExodusInviteLink);

        [Route("InviteBot")]
        public IActionResult InviteBot() =>
            RedirectPermanent(_options.Value.BotInviteLink);
        #endregion
    }
}
