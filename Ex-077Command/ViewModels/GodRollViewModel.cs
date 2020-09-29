using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using WebSite.Models;

namespace WebSite.ViewModels
{
	public class GodRollViewModel
	{
		public IEnumerable<Item> Weapons { get; set; } = Enumerable.Empty<Item>();
	}
}
