using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers
{
	public class CommandController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}