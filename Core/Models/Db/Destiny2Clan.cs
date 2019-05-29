using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.Db
{
	public class Destiny2Clan
	{
		[Display(Name = "ID")]
		public long Id { get; set; }
		[Display(Name = "Название")]
		public string Name { get; set; }
		[Display(Name = "Дата создания")]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy}")]
		public DateTimeOffset CreateDate { get; set; }
		[Display(Name = "Девиз")]
		public string Motto { get; set; }
		[Display(Name = "О клане")]
		public string About { get; set; }
		[Display(Name = "Стражей")]
		public long MemberCount { get; set; }

		public virtual ICollection<Destiny2Clan_Member> Members { get; set; }
	}
}
