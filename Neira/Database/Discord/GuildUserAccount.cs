using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neira.Database
{
	public class GuildUserAccount
	{
		[Key]
		public int Id { get; set; }
		//User id
		public ulong UserId { get; set; }
		public ulong GuildId { get; set; }

		public uint Reputation { get; set; } = 0;

		public uint XP { get; set; } = 0;

		public DateTime LastRep { get; set; } = DateTime.UtcNow.AddDays(-2);

		public DateTime LastXPMessage { get; set; } = DateTime.UtcNow;

		public uint LevelNumber
		{
			get
			{
				return (uint)Math.Sqrt(XP / 50);
			}
		}
	}
}
