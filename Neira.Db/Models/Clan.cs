using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neira.Db.Models
{
	public class Clan
	{
		[Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long Id { get; set; }
		public string Name { get; set; }
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy}")]
		public DateTimeOffset CreateDate { get; set; }
		public string Motto { get; set; }
		public string About { get; set; }
		public long MemberCount { get; set; }
		public virtual ICollection<Clan_Member> Members { get; set; }
	}
}
