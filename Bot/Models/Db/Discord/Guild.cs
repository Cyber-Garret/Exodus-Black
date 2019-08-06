using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Models.Db.Discord
{
	public class Guild : IAccount
	{
		[Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong Id { get; set; }
		public ulong NotificationChannel { get; set; } = 0;
		public bool EnableNotification { get; set; } = false;
		public ulong LoggingChannel { get; set; } = 0;
		public bool EnableLogging { get; set; } = false;
		public string WelcomeMessage { get; set; }
		public string LeaveMessage { get; set; }
		public ulong AutoroleID { get; set; } = 0;
		public string CommandPrefix { get; set; }
	}
}
