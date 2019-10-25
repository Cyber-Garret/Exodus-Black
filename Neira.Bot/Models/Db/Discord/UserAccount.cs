using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neira.Bot.Models.Db
{
	public class UserAccount
	{
		//User Discord ID
		[Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong Id { get; set; }
		// User Money
		public ulong Glimmer { get; set; } = 0;
		public DateTime LastDaily { get; set; } = DateTime.UtcNow.AddDays(-2);
		public uint XP { get; set; } = 0;
		public DateTime LastXPMessage { get; set; } = DateTime.UtcNow;
		public uint LevelNumber
		{
			get
			{
				return (uint)Math.Sqrt(XP / 50);
			}
		}
		public DateTime LastMessage { get; set; } = DateTime.UtcNow;

		public uint CommonEngrams { get; set; } = 0;
		public uint UncommonEngrams { get; set; } = 0;
		public uint RareEngrams { get; set; } = 0;
		public uint LegendaryEngrams { get; set; } = 0;
		public uint ExoticEngrams { get; set; } = 0;
	}
}
