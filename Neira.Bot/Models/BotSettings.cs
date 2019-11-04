namespace Neira.Bot.Models
{
	public class BotSettings
	{
		public string BotToken { get; set; } = "PUT_YOU_DISCORD_BOT_TOKEN_HERE";
		public ulong HomeDiscordServerId { get; set; } = 521689023962415104;
		public string ConnectionString { get; set; } = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;";
	}
}
