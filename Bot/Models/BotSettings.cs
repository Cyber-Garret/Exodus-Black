namespace Bot.Models
{
	public class BotSettings
	{
		public string BotName { get; set; } = "Neuromatrix";
		public string RadioModuleName { get; set; } = "Радиомодуль Нейра#0";
		public DiscordSettings Discord { get; set; } = new DiscordSettings();
	}

	public class DiscordSettings
	{
		public string BotToken { get; set; } = "PUT_YOU_DISCORD_BOT_TOKEN_HERE";
	}
}
