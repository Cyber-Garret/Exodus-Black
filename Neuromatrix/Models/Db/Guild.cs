using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Neuromatrix.Models.Db
{
    public class Guild
    {
        [Key]
        public ulong ID { get; set; }
        public ulong GuildOwnerId { get; set; }
        public ulong NotificationChannel { get; set; }
        public bool EnableLogging { get; set; }
        public ulong LoggingChannel { get; set; }
    }
}
