using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class CommandController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

		public IActionResult Basic()
		{
			return View();
		}

		public IActionResult Special()
		{
			return View();
		}
    }
}