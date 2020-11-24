using Discord.Addons.Interactive.Properties;

using System;

namespace Discord.Addons.Interactive
{
	public enum JumpDisplayOptions
	{
		Never,
		WithManageMessages,
		Always
	}

	/// <summary>
	/// The paginated appearance options.
	/// </summary>
	public class PaginatedAppearanceOptions
	{
		public IEmote Back = new Emoji(Resources.EmoteBack);
		public IEmote First = new Emoji(Resources.EmoteFirst);
		public IEmote Info = new Emoji(Resources.EmoteInfo);
		public IEmote Jump = new Emoji(Resources.EmoteJump);
		public IEmote Last = new Emoji(Resources.EmoteLast);
		public IEmote Next = new Emoji(Resources.EmoteNext);
		public IEmote Stop = new Emoji(Resources.EmoteStop);

		public string FooterFormat = Resources.PaginatedOptionsFooterFormat;
		public string InformationText = Resources.PaginatedOptionsInformationText;

		public JumpDisplayOptions JumpDisplayOptions = JumpDisplayOptions.Never;
		public bool DisplayInformationIcon = true;

		/// <summary>
		/// Default value is 10 min.
		/// </summary>
		public TimeSpan? Timeout = TimeSpan.FromMinutes(10);
		public TimeSpan InfoTimeout = TimeSpan.FromSeconds(30);

		public static PaginatedAppearanceOptions Default { get; set; } = new PaginatedAppearanceOptions();
	}
}
