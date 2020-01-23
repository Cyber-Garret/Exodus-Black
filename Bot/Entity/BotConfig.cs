namespace Bot.Entity
{
	/// <summary>
	/// Represent bot settings from appsettings.json
	/// </summary>
	internal class BotConfig
	{
		/// <summary>
		/// Discord Bot Token from https://discordapp.com/developers/applications/
		/// </summary>
		internal string Token { get; set; } = null;

		/// <summary>
		/// Prefix for bot commands
		/// </summary>
		internal char Prefix { get; set; } = '!';
	}
}
