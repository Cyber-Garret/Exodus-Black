using System.Collections.Generic;
using System.Globalization;

namespace Web.Models
{
	public class CultureSwitcherModel
	{
		public CultureInfo CurrentUICulture { get; set; }
		public List<CultureInfo> SupportedCultures { get; set; }
	}
}
