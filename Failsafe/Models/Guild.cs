using System.Collections.Generic;
using System.Globalization;

namespace Failsafe.Models
{
	public class Guild
	{
		public ulong Id { get; set; }
		public ulong NotificationChannel { get; set; }
		public ulong LoggingChannel { get; set; }
		public ulong WelcomeChannel { get; set; }
		public string WelcomeMessage { get; set; }
		public ulong AutoroleId { get; set; }
		public string CommandPrefix { get; set; }
		public CultureInfo Language { get; set; } = new("ru-RU");
		public string TimeZone { get; set; } = "Europe/Moscow";
		public string GlobalMention { get; set; } = "@here";
		public ulong SelfRoleMessageId { get; set; }
		public List<GuildSelfRole> GuildSelfRoles { get; set; } = new();
	}

	public class GuildSelfRole
	{
		public ulong EmoteId { get; set; }
		public ulong RoleId { get; set; }
	}
}
