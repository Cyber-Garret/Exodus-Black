using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Web.Models
{
	public class Config
	{
		public string BotToken { get; set; }
		public ulong NeiraHomeServerId { get; set; } = 521689023962415104;
		public ulong HellHoundDiscordServer { get; set; } = 513825031525105684;
	}
}
