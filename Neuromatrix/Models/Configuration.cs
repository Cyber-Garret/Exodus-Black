namespace DiscordBot.Models
{
    public class Configuration
    {
        public string Token { get; set; }
        public ulong Guild { get; set; }
        public ulong XurChannel { get; set; }
        public string Version { get; set; }
        public string DbLocation { get; set; }
    }
}
