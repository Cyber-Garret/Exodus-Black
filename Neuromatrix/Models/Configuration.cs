namespace Neuromatrix.Models
{
    public class Configuration
    {
        public string Token { get; set; }
        public ulong Owner { get; set; }
        public ulong Guild { get; set; }
        public ulong XurChannel { get; set; }
        public string Version { get; set; }
        public string DbLocation { get; set; }
    }
}
