using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.Db
{
	public class RaidInfo
	{
		[Key]
		public int Id { get; set; }
		[MaxLength(100)]
		public string Name { get; set; }
		[MaxLength(50)]
		public string Alias { get; set; }
		public int Count { get; set; }
		[MaxLength(1024)]
		public string PreviewDesc { get; set; }

		public List<ActiveRaid> ActiveRaids { get; set; }
	}
}
