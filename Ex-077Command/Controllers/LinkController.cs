using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ex077.Controllers
{
	public class LinkController : Controller
	{
		[Route("GeForceNow")]
		public IActionResult GeforceNow() =>
			RedirectPermanent("https://ad.admitad.com/g/wxfnqla9g4846e6131458cd5089896/");
	}
}
