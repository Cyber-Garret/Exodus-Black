using System.Collections.Generic;

namespace Tangle.Entities.ViewModels
{
	public class IndexViewModel
	{
		/// <summary>
		/// Collection of Destiny 2 weapons
		/// </summary>
		public IEnumerable<Weapon> Weapons { get; set; }
	}
}
