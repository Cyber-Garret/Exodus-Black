namespace Neira.Bot.Models
{
	public class BotSettings
	{
		public string BotName { get; set; } = "Failsafe";
		public string RadioModuleName { get; set; } = "Радиомодуль Нейра#0";
		public DiscordSettings Discord { get; set; } = new DiscordSettings();
		public MySQLSettings MySQL { get; set; } = new MySQLSettings();
	}

	public class DiscordSettings
	{
		public string BotToken { get; set; } = "PUT_YOU_DISCORD_BOT_TOKEN_HERE";
	}

	public class MySQLSettings
	{
		public string Server { get; set; } = "localhost";
		public string Database { get; set; } = "Place_Database_Name_Here";
		public string User { get; set; } = "Place_User_Name_Here";
		public string Password { get; set; } = "Place_Password_Here";
	}
}
