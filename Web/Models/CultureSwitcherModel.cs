using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Models
{
	public class CultureSwitcherModel
	{
		public CultureInfo CurrentUICulture { get; set; }
		public List<CultureInfo> SupportedCultures { get; set; }
	}
}
