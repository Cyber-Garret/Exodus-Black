using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.Models.Db
{
	public class Catalyst
	{
		public int Id { get; set; }

		[MaxLength(256)]
		public string WeaponName { get; set; }

		[MaxLength(1000)]
		public string Icon { get; set; }

		[MaxLength(2048)]
		public string Description { get; set; }

		[MaxLength(1024)]
		public string DropLocation { get; set; }

		[MaxLength(1024)]
		public string Quest { get; set; }

		[MaxLength(1024)]
		public string Masterwork { get; set; }
	}
}
