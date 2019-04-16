namespace Neuromatrix.Models.Db
{
    public class Guild
    {
        public ulong ID { get; set; }
        public ulong NotificationChannel { get; set; }
        public ulong LoggingChannel { get; set; }
        public bool EnableLogging { get; set; }
        public bool EnableNotification { get; set; }
        public string WelcomeMessage { get; set; }
    }
}
