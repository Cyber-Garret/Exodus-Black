using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Web.Models.NeiraLink
{
	public class Clan_Member_Stat
	{
		[Key]
		public int Id { get; set; }
		public int MemberId { get; set; }
		public DateTime Date { get; set; } = DateTime.Now;
		/// <summary>
		/// Time in seconds
		/// </summary>
		public int PlayedTime { get; set; } = 0;
	}
}
