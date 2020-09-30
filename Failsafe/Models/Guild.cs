using System.Collections.Generic;
using System.Globalization;

namespace Failsafe.Models
{
	public class Guild
	{
		public ulong Id { get; set; }
		public ulong NotificationChannel { get; set; } = 0;
		public ulong LoggingChannel { get; set; } = 0;
		public ulong WelcomeChannel { get; set; } = 0;
		public string WelcomeMessage { get; set; } = null;
		public string LeaveMessage { get; set; } = null;
		public ulong AutoroleID { get; set; } = 0;
		public string CommandPrefix { get; set; } = null;
		public CultureInfo Language { get; set; } = new CultureInfo("ru-RU");
		public string TimeZone { get; set; } = "Europe/Moscow";
		public string GlobalMention { get; set; } = "@here";
		public ulong SelfRoleMessageId { get; set; } = 0;
		public List<GuildSelfRole> GuildSelfRoles { get; set; } = new List<GuildSelfRole>();
	}

	public class GuildSelfRole
	{
		public ulong EmoteID { get; set; }
		public ulong RoleID { get; set; }
	}
}
