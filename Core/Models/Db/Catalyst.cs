using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models.Db
{
	public class Catalyst
	{
		public int Id { get; set; }
		public int CategoryId { get; set; }
		public Catalyst_Category Category { get; set; }
		public string WeaponName { get; set; }
		public string Description { get; set; }
		public string CatalystLocation { get; set; }
		public string CatalystQuest { get; set; }
		public string CatalystBonus { get; set; }
	}
}
