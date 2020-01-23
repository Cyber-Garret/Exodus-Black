namespace Bot.Entity
{
	internal class Guild
	{
		internal ulong Id { get; set; } = 0;
		internal ulong NotificationChannel { get; set; } = 0;
		internal ulong LoggingChannel { get; set; } = 0;
		internal ulong WelcomeChannel { get; set; } = 0;
		internal string WelcomeMessage { get; set; } = string.Empty;
		internal ulong AutoroleID { get; set; } = 0;
		internal string GlobalMention { get; set; } = string.Empty;
		internal ulong SelfRoleMessageId { get; set; } = 0;
	}
}