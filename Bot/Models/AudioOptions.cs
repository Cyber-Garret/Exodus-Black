using Discord;

namespace Bot.Models
{
	public class AudioOptions
	{
		public IUser Master { get; set; }
		public bool Shuffle { get; set; }
		public bool RepeatTrack { get; set; }
	}
}
