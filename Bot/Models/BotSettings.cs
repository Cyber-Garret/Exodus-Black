namespace Bot.Models
{
	public class BotSettings
	{
		public string BotName { get; set; } = "Neuromatrix";

		public DiscordSettings Discord { get; set; } = new DiscordSettings();
	}

	public class DiscordSettings
	{
		public string BotToken { get; set; } = "PUT_YOU_DISCORD_BOT_TOKEN_HERE";
		public int[] Shards { get; set; } = { 0, 1 };
	}
}
