﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Models.Db.Discord
{
    public class Guild : IAccount
    {
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }
        public ulong NotificationChannel { get; set; } = 0;
        public ulong LoggingChannel { get; set; } = 0;
        public ulong WelcomeChannel { get; set; } = 0;
        public string WelcomeMessage { get; set; }
        public string LeaveMessage { get; set; }
        public ulong AutoroleID { get; set; } = 0;
        public string CommandPrefix { get; set; }
    }
}
