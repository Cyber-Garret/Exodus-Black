using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Neira.Database
{
	public class ADOnline
	{
		[Key]
		public int Id { get; set; }
		public int Online { get; set; }
		public int Idle { get; set; }
		public int DnD { get; set; }
		public int InVoice { get; set; }
		public int InGame { get; set; }
		public int InDestiny2 { get; set; }
		public DateTime Date { get; set; }
	}
}
