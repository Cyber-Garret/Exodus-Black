using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.Models.Db
{
	public class Catalyst
	{
		public int Id { get; set; }

		[Display(Name ="Оружие"), MaxLength(256)]
		public string WeaponName { get; set; }

		[Display(Name ="Иконка"),MaxLength(1000)]
		public string Icon { get; set; }

		[Display(Name ="Описание"),MaxLength(2048)]
		public string Description { get; set; }

		[Display(Name ="Источник"),MaxLength(1024)]
		public string DropLocation { get; set; }

		[Display(Name = "Задание"), MaxLength(1024)]
		public string Quest { get; set; }

		[Display(Name = "Бонус"), MaxLength(1024)]
		public string Masterwork { get; set; }
	}
}
