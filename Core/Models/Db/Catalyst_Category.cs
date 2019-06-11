using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models.Db
{
	public class Catalyst_Category
	{
		public int Id { get; set; }
		public string Value { get; set; }
		public IEnumerable<Catalyst> Catalysts { get; set; }
	}
}
