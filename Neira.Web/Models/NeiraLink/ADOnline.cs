using System;
using System.ComponentModel.DataAnnotations;

namespace Neira.Web.Models.NeiraLink
{
	public class ADOnline
	{

		[Key]
		public int Id { get; set; }
		public int Online { get; set; }
		public int Idle { get; set; }
		public int DnD { get; set; }
		public int InVoice { get; set; }
		public int AFK { get; set; }
		public int InGame { get; set; }
		public int InDestiny2 { get; set; }
		public DateTime Date { get; set; }
	}
}
