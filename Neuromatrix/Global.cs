using Discord.WebSocket;

namespace DiscordBot
{
    internal static class Global
    {
        //internal static DiscordShardedClient Client { get; set; }
        internal static string Token { get; set; }
        internal static ulong Guild { get; set; }
        internal static ulong XurChannel { get; set; }
        internal static string Version { get; set; }
        internal static string DbLocation { get; set; }
    }
}
