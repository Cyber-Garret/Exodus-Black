using DiscordBot.Features.Catalyst;
using System;
using System.Collections.Generic;

namespace DiscordBot
{
	internal static class Global
	{
		internal static string Token { get; set; }
		internal static string Version { get; set; }
		internal static readonly string InvisibleString = "\u200b";
		internal static Random Rng { get; set; } = new Random();
		internal static List<CatalystCore> CatalystMessages { get; set; } = new List<CatalystCore>();

	}
}
