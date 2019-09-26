using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neira.Db.Models
{
	public class Guild : IAccount
	{
		[Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong Id { get; set; }
		public ulong NotificationChannel { get; set; } = 0;
		public ulong LoggingChannel { get; set; } = 0;
		public ulong WelcomeChannel { get; set; } = 0;
		public string WelcomeMessage { get; set; }
		public string LeaveMessage { get; set; }
		public ulong AutoroleID { get; set; } = 0;
		public string CommandPrefix { get; set; }
		public string GlobalMention { get; set; } = "@here";
	}
}
