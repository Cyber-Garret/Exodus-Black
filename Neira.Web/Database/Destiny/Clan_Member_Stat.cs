using System;
using System.ComponentModel.DataAnnotations;

namespace Neira.Web.Database
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
